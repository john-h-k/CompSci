// CppPlayground.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <Windows.h>

#define EXPORT_ASM extern "C" __declspec(nothrow)  __declspec(dllexport) __declspec(naked) auto __stdcall

EXPORT_ASM UsesTest(uint64_t) noexcept -> uint64_t
{
	__asm
	{
		mov r9, 10000000

		loop:

		test rcx, 1
		setz al

		dec r9
		jnz loop

		ret
	}
}

EXPORT_ASM UsesBitwise(uint64_t) noexcept -> uint64_t
{
	__asm
	{
		mov r9, 10000000

		loop:

		not rcx
		and rcx, 1
		mov rax, rcx

		dec r9
		jnz loop

		ret
	}
}

EXPORT_ASM UsesDiv(uint64_t) noexcept -> uint64_t
{
	__asm
	{
		mov r9, 10000000

		loop:

		xor rdx, rdx
		mov rax, rcx
		mov r8, 2
		div r8
		mov rax, rdx
		xor rax, 1

		dec r9
		jnz loop

		ret
	}
}

__forceinline __declspec(naked) long GetTime()
{
	__asm
	{
		rdtscp
		shl rdx, 32
		or rax, rdx

		ret
	}
}

int main()
{
	std::cout << "Use C# PInvoke file, this is a deprecated main" << std::endl;
	getchar();
}