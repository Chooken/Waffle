#pragma once

#ifdef WF_PLATFORM_WINDOWS

extern Waffle::Application* Waffle::CreateApplication();

int main(int argc, char** argv)
{
	auto app = Waffle::CreateApplication();

	app->Run();

	delete app;
}

#endif // WF_PLATFORM_WINDOWS