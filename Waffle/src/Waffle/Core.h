#pragma once

#ifdef WF_PLATFORM_WINDOWS
	#ifdef WF_BUILD_DLL

		#define WAFFLE_API __declspec(dllexport)

	#else

		#define WAFFLE_API __declspec(dllimport)

	#endif

#else

	#error Waffle only supports Windows!

#endif