using Raylib_cs;

void Render()
{
    Raylib.InitWindow(800, 480, "Waffle Engine");

    while (!Raylib.WindowShouldClose())
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);

        Raylib.DrawText("Hello, world!", 12, 12, 20, Color.Black);

        Raylib.EndDrawing();
    }

    Raylib.CloseWindow();
}

Render();