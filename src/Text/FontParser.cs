namespace WaffleEngine;

// Sebastian Legues Font Parser with some minor preference tweaks.
public static class FontParser
{
    public static TrueTypeFontData Parse(string path_to_font)
    {
        using FontReader reader = new(path_to_font);

        // --- Get table locations ---
        Dictionary<string, uint> table_location_lookup = ReadTableLocations(reader);
        uint glyph_table_location = table_location_lookup["glyf"];
        uint loca_table_location = table_location_lookup["loca"];
        uint cmap_location = table_location_lookup["cmap"];

        // ---- Read Head Table ----
        reader.GoTo(table_location_lookup["head"]);
        reader.SkipBytes(18);
        // Design units to Em size (range from 64 to 16384)
        int units_per_em = reader.ReadUInt16();
        reader.SkipBytes(30);
        // Number of bytes used by the offsets in the 'loca' table (for looking up glyph locations)
        int num_bytes_per_location_lookup = (reader.ReadInt16() == 0 ? 2 : 4);

        // --- Read 'maxp' table ---
        reader.GoTo(table_location_lookup["maxp"]);
        reader.SkipBytes(4);
        int num_glyphs = reader.ReadUInt16();
        uint[] glyph_locations = GetAllGlyphLocations(reader, num_glyphs, num_bytes_per_location_lookup, loca_table_location, glyph_table_location);

        GlyphMap[] mappings = GetUnicodeToGlyphIndexMappings(reader, cmap_location);
        TrueTypeGlyphData[] glyphs = ReadAllGlyphs(reader, glyph_locations, mappings);

        ApplyLayoutInfo();


        TrueTypeFontData true_type_font_data = new(glyphs, units_per_em);
        return true_type_font_data;

        // Get horizontal layout information from the "hhea" and "hmtx" tables
        void ApplyLayoutInfo()
        {
            (int advance, int left)[] layout_data = new (int, int)[num_glyphs];

            // Get number of metrics from the 'hhea' table
            reader.GoTo(table_location_lookup["hhea"]);

            reader.SkipBytes(8); // unused: version, ascent, descent
            int line_gap = reader.ReadInt16();
            int advance_width_max = reader.ReadInt16();
            reader.SkipBytes(22); // unused: minLeftSideBearing, minRightSideBearing, xMaxExtent, caretSlope/Offset, reserved, metricDataFormat
            int num_advance_width_metrics = reader.ReadInt16();

            // Get the advance width and leftsideBearing metrics from the 'hmtx' table
            reader.GoTo(table_location_lookup["hmtx"]);
            int last_advance_width = 0;

            for (int i = 0; i < num_advance_width_metrics; i++)
            {
                int advance_width = reader.ReadUInt16();
                int left_side_bearing = reader.ReadInt16();
                last_advance_width = advance_width;

                layout_data[i] = (advance_width, left_side_bearing);
            }

            // Some fonts have a run of monospace characters at the end
            int num_rem = num_glyphs - num_advance_width_metrics;

            for (int i = 0; i < num_rem; i++)
            {
                int left_side_bearing = reader.ReadInt16();
                int glyph_index = num_advance_width_metrics + i;

                layout_data[glyph_index] = (last_advance_width, left_side_bearing);
            }

            // Apply
            foreach (var c in glyphs)
            {
                c.AdvanceWidth = layout_data[c.GlyphIndex].advance;
                c.LeftSideBearing = layout_data[c.GlyphIndex].left;
            }
        }
    }


    // -- Read Font Directory to create a lookup of table locations by their 4-character nametag --
    static Dictionary<string, uint> ReadTableLocations(FontReader reader)
    {
        Dictionary<string, uint> table_locations = new();

        // -- offset subtable --
        reader.SkipBytes(4); // unused: scalerType
        int num_tables = reader.ReadUInt16();
        reader.SkipBytes(6); // unused: searchRange, entrySelector, rangeShift

        // -- table directory --
        for (int i = 0; i < num_tables; i++)
        {
            string tag = reader.ReadString(4);
            uint checksum = reader.ReadUInt32();
            uint offset = reader.ReadUInt32();
            uint length = reader.ReadUInt32();

            table_locations.Add(tag, offset);

        }
        return table_locations;
    }

    static TrueTypeGlyphData[] ReadAllGlyphs(FontReader reader, uint[] glyph_locations, GlyphMap[] mappings)
    {
        TrueTypeGlyphData[] glyphs = new TrueTypeGlyphData[mappings.Length];

        for (int i = 0; i < mappings.Length; i++)
        {
            GlyphMap mapping = mappings[i];

            TrueTypeGlyphData true_type_glyph_data = ReadGlyph(reader, glyph_locations, mapping.GlyphIndex);
            true_type_glyph_data.UnicodeValue = mapping.Unicode;
            glyphs[i] = true_type_glyph_data;
        }

        return glyphs;
    }

    static TrueTypeGlyphData ReadGlyph(FontReader reader, uint[] glyph_locations, uint glyph_index)
    {
        uint glyph_location = glyph_locations[glyph_index];

        reader.GoTo(glyph_location);
        int contour_count = reader.ReadInt16();

        // Glyph is either simple or compound
        // * Simple: outline data is stored here directly
        // * Compound: two or more simple glyphs need to be looked up, transformed, and combined
        bool is_simple_glyph = contour_count >= 0;

        if (is_simple_glyph) return ReadSimpleGlyph(reader, glyph_locations, glyph_index);
        else return ReadCompoundGlyph(reader, glyph_locations, glyph_index);
    }

    // Read a simple glyph from the 'glyf' table
    static TrueTypeGlyphData ReadSimpleGlyph(FontReader reader, uint[] glyph_locations, uint glyph_index)
    {
        // Flag masks
        const int OnCurve = 0;
        const int IsSingleByteX = 1;
        const int IsSingleByteY = 2;
        const int Repeat = 3;
        const int InstructionX = 4;
        const int InstructionY = 5;

        reader.GoTo(glyph_locations[glyph_index]);

        TrueTypeGlyphData true_type_glyph_data = new();
        true_type_glyph_data.GlyphIndex = glyph_index;

        int contour_count = reader.ReadInt16();
        if (contour_count < 0) throw new Exception("Expected simple glyph, but found compound glyph instead");

        true_type_glyph_data.MinX = reader.ReadInt16();
        true_type_glyph_data.MinY = reader.ReadInt16();
        true_type_glyph_data.MaxX = reader.ReadInt16();
        true_type_glyph_data.MaxY = reader.ReadInt16();

        // Read contour ends
        int num_points = 0;
        int[] contour_end_indices = new int[contour_count];

        for (int i = 0; i < contour_count; i++)
        {
            int contour_end_index = reader.ReadUInt16();
            num_points = System.Math.Max(num_points, contour_end_index + 1);
            contour_end_indices[i] = contour_end_index;
        }

        int instructions_length = reader.ReadInt16();
        reader.SkipBytes(instructions_length); // skip instructions (hinting stuff)

        byte[] all_flags = new byte[num_points];
        Point[] points = new Point[num_points];

        for (int i = 0; i < num_points; i++)
        {
            byte flag = reader.ReadByte();
            all_flags[i] = flag;

            if (FlagBitIsSet(flag, Repeat))
            {
                int repeat_count = reader.ReadByte();

                for (int r = 0; r < repeat_count; r++)
                {
                    i++;
                    all_flags[i] = flag;
                }
            }
        }

        ReadCoords(true);
        ReadCoords(false);
        true_type_glyph_data.Points = points;
        true_type_glyph_data.ContourEndIndices = contour_end_indices;
        return true_type_glyph_data;

        void ReadCoords(bool reading_x)
        {
            int min = int.MaxValue;
            int max = int.MinValue;

            int single_byte_flag_bit = reading_x ? IsSingleByteX : IsSingleByteY;
            int instruction_flag_mask = reading_x ? InstructionX : InstructionY;

            int coord_val = 0;

            for (int i = 0; i < num_points; i++)
            {
                byte curr_flag = all_flags[i];

                // Offset value is represented with 1 byte (unsigned)
                // Here the instruction flag tells us whether to add or subtract the offset
                if (FlagBitIsSet(curr_flag, single_byte_flag_bit))
                {
                    int coord_offset = reader.ReadByte();
                    bool positive_offset = FlagBitIsSet(curr_flag, instruction_flag_mask);
                    coord_val += positive_offset ? coord_offset : -coord_offset;
                }
                // Offset value is represented with 2 bytes (signed)
                // Here the instruction flag tells us whether an offset value exists or not
                else if (!FlagBitIsSet(curr_flag, instruction_flag_mask))
                {
                    coord_val += reader.ReadInt16();
                }

                if (reading_x) points[i].X = coord_val;
                else points[i].Y = coord_val;
                points[i].OnCurve = FlagBitIsSet(curr_flag, OnCurve);

                min = System.Math.Min(min, coord_val);
                max = System.Math.Max(max, coord_val);
            }
        }
    }

    static TrueTypeGlyphData ReadCompoundGlyph(FontReader reader, uint[] glyph_locations, uint glyph_index)
    {
        TrueTypeGlyphData compound_true_type_glyph = new();
        compound_true_type_glyph.GlyphIndex = glyph_index;

        uint glyph_location = glyph_locations[glyph_index];
        reader.GoTo(glyph_location);
        reader.SkipBytes(2);

        compound_true_type_glyph.MinX = reader.ReadInt16();
        compound_true_type_glyph.MinY = reader.ReadInt16();
        compound_true_type_glyph.MaxX = reader.ReadInt16();
        compound_true_type_glyph.MaxY = reader.ReadInt16();

        List<Point> all_points = new();
        List<int> all_contour_end_indices = new();

        while (true)
        {
            (TrueTypeGlyphData component_glyph, bool has_more_glyphs) = ReadNextComponentGlyph(reader, glyph_locations, glyph_location);

            // Add all contour end indices from the simple glyph component to the compound glyph's data
            // Note: indices must be offset to account for previously-added component glyphs
            foreach (int end_index in component_glyph.ContourEndIndices)
            {
                all_contour_end_indices.Add(end_index + all_points.Count);
            }
            all_points.AddRange(component_glyph.Points);

            if (!has_more_glyphs) break;
        }

        compound_true_type_glyph.Points = all_points.ToArray();
        compound_true_type_glyph.ContourEndIndices = all_contour_end_indices.ToArray();
        return compound_true_type_glyph;
    }

    static (TrueTypeGlyphData glyph, bool hasMoreGlyphs) ReadNextComponentGlyph(FontReader reader, uint[] glyph_locations, uint glyph_location)
    {
        uint flag = reader.ReadUInt16();
        uint glyph_index = reader.ReadUInt16();

        uint component_glyph_location = glyph_locations[glyph_index];
        // If compound glyph refers to itself, return empty glyph to avoid infinite loop.
        // Had an issue with this on the 'carriage return' character in robotoslab.
        // There's likely a bug in my parsing somewhere, but this is my work-around for now...
        if (component_glyph_location == glyph_location)
        {
            return (new TrueTypeGlyphData() { Points = Array.Empty<Point>(), ContourEndIndices = Array.Empty<int>() }, false);
        }

        // Decode flags
        bool args_are2_bytes = FlagBitIsSet(flag, 0);
        bool args_are_xy_values = FlagBitIsSet(flag, 1);
        bool round_xy_to_grid = FlagBitIsSet(flag, 2);
        bool is_single_scale_value = FlagBitIsSet(flag, 3);
        bool is_more_components_after_this = FlagBitIsSet(flag, 5);
        bool is_x_and_y_scale = FlagBitIsSet(flag, 6);
        bool is2_x2_matrix = FlagBitIsSet(flag, 7);
        bool has_instructions = FlagBitIsSet(flag, 8);
        bool use_this_component_metrics = FlagBitIsSet(flag, 9);
        bool components_overlap = FlagBitIsSet(flag, 10);

        // Read args (these are either x/y offsets, or point number)
        int arg1 = args_are2_bytes ? reader.ReadInt16() : reader.ReadSByte();
        int arg2 = args_are2_bytes ? reader.ReadInt16() : reader.ReadSByte();

        if (!args_are_xy_values) throw new Exception("TODO: Args1&2 are point indices to be matched, rather than offsets");

        double offset_x = arg1;
        double offset_y = arg2;

        double i_hat_x = 1;
        double i_hat_y = 0;
        double j_hat_x = 0;
        double j_hat_y = 1;

        if (is_single_scale_value)
        {
            i_hat_x = reader.ReadFixedPoint2Dot14();
            j_hat_y = i_hat_x;
        }
        else if (is_x_and_y_scale)
        {
            i_hat_x = reader.ReadFixedPoint2Dot14();
            j_hat_y = reader.ReadFixedPoint2Dot14();
        }
        // Todo: incomplete implemntation
        else if (is2_x2_matrix)
        {
            i_hat_x = reader.ReadFixedPoint2Dot14();
            i_hat_y = reader.ReadFixedPoint2Dot14();
            j_hat_x = reader.ReadFixedPoint2Dot14();
            j_hat_y = reader.ReadFixedPoint2Dot14();
        }

        uint current_compound_glyph_read_location = reader.GetLocation();
        TrueTypeGlyphData simple_true_type_glyph = ReadGlyph(reader, glyph_locations, glyph_index);
        reader.GoTo(current_compound_glyph_read_location);

        for (int i = 0; i < simple_true_type_glyph.Points.Length; i++)
        {
            (double x_prime, double y_prime) = TransformPoint(simple_true_type_glyph.Points[i].X, simple_true_type_glyph.Points[i].Y);
            simple_true_type_glyph.Points[i].X = (int)x_prime;
            simple_true_type_glyph.Points[i].Y = (int)y_prime;
        }

        return (simple_true_type_glyph, is_more_components_after_this);

        (double xPrime, double yPrime) TransformPoint(double x, double y)
        {
            double x_prime = i_hat_x * x + j_hat_x * y + offset_x;
            double y_prime = i_hat_y * x + j_hat_y * y + offset_y;
            return (x_prime, y_prime);
        }
    }


    static uint[] GetAllGlyphLocations(FontReader reader, int num_glyphs, int bytes_per_entry, uint loca_table_location, uint glyf_table_location)
    {
        uint[] all_glyph_locations = new uint[num_glyphs];
        bool is_two_byte_entry = bytes_per_entry == 2;

        for (int glyph_index = 0; glyph_index < num_glyphs; glyph_index++)
        {
            reader.GoTo(loca_table_location + glyph_index * bytes_per_entry);
            // If 2-byte format is used, the stored location is half of actual location (so multiply by 2)
            uint glyph_data_offset = is_two_byte_entry ? reader.ReadUInt16() * 2u : reader.ReadUInt32();
            all_glyph_locations[glyph_index] = glyf_table_location + glyph_data_offset;
        }

        return all_glyph_locations;
    }

    // Create a lookup from unicode to font's internal glyph index
    static GlyphMap[] GetUnicodeToGlyphIndexMappings(FontReader reader, uint cmap_offset)
    {
        List<GlyphMap> glyph_pairs = new();
        reader.GoTo(cmap_offset);

        uint version = reader.ReadUInt16();
        uint num_subtables = reader.ReadUInt16(); // font can contain multiple character maps for different platforms

        // --- Read through metadata for each character map to find the one we want to use ---
        uint cmap_subtable_offset = 0;
        int selected_unicode_version_id = -1;

        for (int i = 0; i < num_subtables; i++)
        {
            int platform_id = reader.ReadUInt16();
            int platform_specific_id = reader.ReadUInt16();
            uint offset = reader.ReadUInt32();

            // Unicode encoding
            if (platform_id == 0)
            {
                // Use highest supported unicode version
                if (platform_specific_id is 0 or 1 or 3 or 4 && platform_specific_id > selected_unicode_version_id)
                {
                    cmap_subtable_offset = offset;
                    selected_unicode_version_id = platform_specific_id;
                }
            }
            // Microsoft Encoding
            else if (platform_id == 3 && selected_unicode_version_id == -1)
            {
                if (platform_specific_id is 1 or 10)
                {
                    cmap_subtable_offset = offset;
                }
            }
        }

        if (cmap_subtable_offset == 0)
        {
            throw new Exception("Font does not contain supported character map type (TODO)");
        }

        // Go to the character map
        reader.GoTo(cmap_offset + cmap_subtable_offset);
        int format = reader.ReadUInt16();
        bool has_read_missing_char_glyph = false;

        if (format != 12 && format != 4)
        {
            throw new Exception("Font cmap format not supported (TODO): " + format);
        }

        // ---- Parse Format 4 ----
        if (format == 4)
        {
            int length = reader.ReadUInt16();
            int language_code = reader.ReadUInt16();
            // Number of contiguous segments of character codes
            int seg_count2_x = reader.ReadUInt16();
            int seg_count = seg_count2_x / 2;
            reader.SkipBytes(6); // Skip: searchRange, entrySelector, rangeShift

            // Ending character code for each segment (last = 2^16 - 1)
            int[] end_codes = new int[seg_count];
            for (int i = 0; i < seg_count; i++)
            {
                end_codes[i] = reader.ReadUInt16();
            }

            reader.Skip16BitEntries(1); // Reserved pad

            int[] start_codes = new int[seg_count];
            for (int i = 0; i < seg_count; i++)
            {
                start_codes[i] = reader.ReadUInt16();
            }

            int[] id_deltas = new int[seg_count];
            for (int i = 0; i < seg_count; i++)
            {
                id_deltas[i] = reader.ReadUInt16();
            }

            (int offset, int readLoc)[] id_range_offsets = new (int, int)[seg_count];
            for (int i = 0; i < seg_count; i++)
            {
                int read_loc = (int)reader.GetLocation();
                int offset = reader.ReadUInt16();
                id_range_offsets[i] = (offset, read_loc);
            }

            for (int i = 0; i < start_codes.Length; i++)
            {
                int end_code = end_codes[i];
                int curr_code = start_codes[i];

                if (curr_code == 65535) break; // not sure about this (hack to avoid out of bounds on a specific font)

                while (curr_code <= end_code)
                {
                    int glyph_index;
                    // If idRangeOffset is 0, the glyph index can be calculated directly
                    if (id_range_offsets[i].offset == 0)
                    {
                        glyph_index = (curr_code + id_deltas[i]) % 65536;
                    }
                    // Otherwise, glyph index needs to be looked up from an array
                    else
                    {
                        uint reader_location_old = reader.GetLocation();
                        int range_offset_location = id_range_offsets[i].readLoc + id_range_offsets[i].offset;
                        int glyph_index_array_location = 2 * (curr_code - start_codes[i]) + range_offset_location;

                        reader.GoTo(glyph_index_array_location);
                        glyph_index = reader.ReadUInt16();

                        if (glyph_index != 0)
                        {
                            glyph_index = (glyph_index + id_deltas[i]) % 65536;
                        }

                        reader.GoTo(reader_location_old);
                    }

                    glyph_pairs.Add(new((uint)glyph_index, (uint)curr_code));
                    has_read_missing_char_glyph |= glyph_index == 0;
                    curr_code++;
                }
            }
        }
        // ---- Parse Format 12 ----
        else if (format == 12)
        {
            reader.SkipBytes(10); // Skip: reserved, subtableByteLengthInlcudingHeader, languageCode
            uint num_groups = reader.ReadUInt32();

            for (int i = 0; i < num_groups; i++)
            {
                uint start_char_code = reader.ReadUInt32();
                uint end_char_code = reader.ReadUInt32();
                uint start_glyph_index = reader.ReadUInt32();

                uint num_chars = end_char_code - start_char_code + 1;
                for (int char_code_offset = 0; char_code_offset < num_chars; char_code_offset++)
                {
                    uint char_code = (uint)(start_char_code + char_code_offset);
                    uint glyph_index = (uint)(start_glyph_index + char_code_offset);

                    glyph_pairs.Add(new(glyph_index, char_code));
                    has_read_missing_char_glyph |= glyph_index == 0;
                }
            }
        }

        if (!has_read_missing_char_glyph)
        {
            glyph_pairs.Add(new(0, 65535));
        }

        return glyph_pairs.ToArray();
    }

    static bool FlagBitIsSet(byte flag, int bit_index) => ((flag >> bit_index) & 1) == 1;
    static bool FlagBitIsSet(uint flag, int bit_index) => ((flag >> bit_index) & 1) == 1;

    public readonly struct GlyphMap
    {
        public readonly uint GlyphIndex;
        public readonly uint Unicode;

        public GlyphMap(uint index, uint unicode)
        {
            GlyphIndex = index;
            Unicode = unicode;
        }
    }

    public struct HeadTableData
    {
        public uint UnitsPerEM;
        public uint NumBytesPerGlyphIndexToLocationEntry;

        public HeadTableData(uint units_per_em, uint num_bytes_per_glyph_index_to_location_entry)
        {
            UnitsPerEM = units_per_em;
            NumBytesPerGlyphIndexToLocationEntry = num_bytes_per_glyph_index_to_location_entry;
        }
    }

}