#pragma once

#include "Core.h"

namespace Waffle {

	class WAFFLE_API Application
	{
	public:

		Application();
		virtual ~Application();

		void Run();
	};

	Application* CreateApplication();

}

