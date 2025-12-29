namespace MagicGame.Scripts;

public static class Parser
{
    private static Dictionary<int, Token> _namedInstructions = new();
    
    private ref struct ParseData
    {
        public ReadOnlySpan<char> Source;
        private int _position;

        public ParseData(ReadOnlySpan<char> source)
        {
            Source = source;
            _position = 0;
        }

        public int Position => _position;

        public char Current {
            get
            {
                if (IsEnd) return ' ';
                return Source[_position];
            }
        }

        public bool IsEnd => _position >= Source.Length;

        public ReadOnlySpan<char> Range(Range range) => Source[range];

        public void Progress() => _position++;
    }

    private enum TokenType
    {
        Block,
        Pop,
        Push,
        Call,
        Extern,
        If,
        While,
        String,
        Number,
    }
    
    private struct Token
    {
        public TokenType Type;
        public int FirstPos;
        public int Size;

        public ReadOnlySpan<char> GetSpan(ReadOnlySpan<char> source) => source.Slice(FirstPos, Size);
    }
    
    public static Program Parse(
        string source, 
        Dictionary<string, int> externalFunctionIndexes)
    {
        var blockLocations = new Dictionary<string, int>();
        var instructions = new List<Instruction>();

        var parseData = new ParseData(source);
        
        while (!parseData.IsEnd)
        {
            if (!TryGetToken(ref parseData, out var operatorToken))
            {
                break;
            }
            if (!TryGetToken(ref parseData, out var dataToken))
            {
                break;
            }
            
            Instruction instruction = new Instruction();

            switch (operatorToken.Type)
            {
                case TokenType.Block:
                    blockLocations.Add(dataToken.GetSpan(source).ToString(), instructions.Count);
                    instruction.Type = Operation.Block;
                    break;
                case TokenType.Pop:
                    instruction.Type = Operation.Pop;
                    break;
                case TokenType.Push:
                    instruction.Type = Operation.Push;
                    break;
                case TokenType.Call:
                    instruction.Type = Operation.Call;
                    break;
                case TokenType.Extern:
                    instruction.Type = Operation.CallExternal;
                    break;
                case TokenType.If:
                    instruction.Type = Operation.If;
                    break;
                case TokenType.While:
                    instruction.Type = Operation.While;
                    break;
                default:
                    instruction.Type = Operation.Error;
                    break;
            }
            
            switch (dataToken.Type)
            {
                case TokenType.Number:
                    instruction.Data = int.Parse(dataToken.GetSpan(source));
                    break;
                case TokenType.String:
                    _namedInstructions.Add(instructions.Count, dataToken);
                    break;
                default:
                    instruction.Type = Operation.Error;
                    break;
            }
            
            instructions.Add(instruction);
        }

        // Changes Strings to there block addresses
        foreach (var (index, token) in _namedInstructions)
        {
            Instruction instruction = instructions[index];
            
            if (!blockLocations.GetAlternateLookup<ReadOnlySpan<char>>()
                .TryGetValue(token.GetSpan(source), out var location))
            {
                instruction.Type = Operation.Error;
            }

            instruction.Data = location;

            instructions[index] = instruction;
        }
        
        return new Program(instructions, blockLocations);
    }

    private static bool TryGetToken(ref ParseData data, out Token token)
    {
        token = new Token();
        
        while (char.IsWhiteSpace(data.Current))
        {
            if (data.IsEnd)
                return false;
            
            data.Progress();
        }
        
        if (char.IsDigit(data.Current))
        {
            token = GetNumberToken(ref data);
            return true;
        }

        token = GetStringToken(ref data);
        
        switch (token.GetSpan(data.Source))
        {
            case "block":
                token.Type = TokenType.Block;
                break;
            case "pop":
                token.Type = TokenType.Pop;
                break;
            case "push":
                token.Type = TokenType.Push;
                break;
            case "call":
                token.Type = TokenType.Call;
                break;
            case "extern":
                token.Type = TokenType.Extern;
                break;
            case "if":
                token.Type = TokenType.If;
                break;
            case "while":
                token.Type = TokenType.While;
                break;
        }

        return true;
    }
    
    private static Token GetStringToken(ref ParseData data)
    {
        Token token = new Token()
        {
            Type = TokenType.String,
            FirstPos = data.Position,
        };

        while (!data.IsEnd)
        {
            if (char.IsWhiteSpace(data.Current))
                break;

            data.Progress();
        }

        token.Size = data.Position - token.FirstPos;

        return token;
    }
    
    private static Token GetNumberToken(ref ParseData data)
    {
        Token token = new Token()
        {
            Type = TokenType.Number,
            FirstPos = data.Position,
        };

        while (!data.IsEnd)
        {
            if (!char.IsDigit(data.Current))
                break;

            data.Progress();
        }
        
        token.Size = data.Position - token.FirstPos;

        return token;
    }
}