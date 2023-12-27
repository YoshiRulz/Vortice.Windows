// Copyright � Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;

namespace Vortice;

/// <summary>
/// Provides a set of methods to supplement or replace <see cref="Unsafe" /> and <see cref="MemoryMarshal" />.
/// </summary>
public static unsafe class UnsafeUtilities
{
    public static void Read<T>(void* source, T[] values) where T : unmanaged
    {
        int count = values.Length;
        fixed (void* dstPtr = values)
        {
            Unsafe.CopyBlockUnaligned(dstPtr, source, (uint)(count * sizeof(T)));
        }
    }

    public static void Read<T>(void* source, T[] values, int count) where T : unmanaged
    {
        fixed (void* dstPtr = values)
        {
            Unsafe.CopyBlockUnaligned(dstPtr, source, (uint)(count * sizeof(T)));
        }
    }

    public static void Read<T>(IntPtr source, T[] values) where T : unmanaged
    {
        int count = values.Length;
        fixed (void* dstPtr = values)
        {
            Unsafe.CopyBlockUnaligned(dstPtr, (void*)source, (uint)(count * sizeof(T)));
        }
    }

    public static void Read<T>(IntPtr source, T[] values, int count) where T : unmanaged
    {
        fixed (void* dstPtr = values)
        {
            Unsafe.CopyBlockUnaligned(dstPtr, (void*)source, (uint)(count * sizeof(T)));
        }
    }

    public static IntPtr Write<T>(IntPtr destination, ref T value) where T : unmanaged
    {
        fixed (void* valuePtr = &value)
        {
            Unsafe.CopyBlockUnaligned((void*)destination, valuePtr, (uint)(sizeof(T)));
            return destination + sizeof(T);
        }
    }

    public static IntPtr Write<T>(IntPtr destination, T[] data) where T : unmanaged
    {
        int byteCount = data.Length * sizeof(T);
        fixed (void* dataPtr = data)
        {
            Unsafe.CopyBlockUnaligned((void*)destination, dataPtr, (uint)byteCount);
            return destination + byteCount;
        }
    }

    public static IntPtr Write<T>(IntPtr destination, T[] data, int offset, int count) where T : unmanaged
    {
        int byteCount = count * sizeof(T);
        fixed (void* dataPtr = &data[offset])
        {
            Unsafe.CopyBlockUnaligned((void*)destination, dataPtr, (uint)byteCount);
            return destination + byteCount;
        }
    }

    public static void* Alloc(int byteCount) 
    {
#if NET6_0_OR_GREATER
        return NativeMemory.Alloc((nuint)byteCount);
#else
        return (void*)Marshal.AllocHGlobal(byteCount);
#endif
    }

    public static void* Alloc(int elementCount, int elementSize)
    {
#if NET6_0_OR_GREATER
        return NativeMemory.Alloc((nuint)elementCount, (nuint)elementSize);
#else
        return Alloc(elementCount * elementSize);
#endif
    }

    public static T* Alloc<T>() where T : unmanaged
    {
#if NET6_0_OR_GREATER
        return (T*)NativeMemory.Alloc((nuint)sizeof(T));
#else
        return (T*)Marshal.AllocHGlobal(sizeof(T));
#endif
    }

    public static T* Alloc<T>(int elementCount) where T : unmanaged
    {
        return (T*)Alloc(elementCount, sizeof(T));
    }

    public static T* AllocWithData<T>(T source) where T : unmanaged
    {
        int sizeInBytes = sizeof(T);
        T* dest = (T*)Alloc(sizeInBytes);
        Unsafe.CopyBlockUnaligned(dest, &source, (uint)sizeInBytes);

        return dest;
    }

    public static T* AllocWithData<T>(T[] source) where T : unmanaged
    {
        ReadOnlySpan<T> span = source;

        return AllocWithData(span);
    }

    public static T* AllocWithData<T>(ReadOnlySpan<T> source) where T : unmanaged
    {
        int sizeInBytes = source.Length * sizeof(T);
        T* dest = (T*)Alloc(sizeInBytes);
        fixed (T* sourcePtr = source)
        {
            Unsafe.CopyBlockUnaligned(dest, sourcePtr, (uint)sizeInBytes);
        }

        return dest;
    }

    public static void Free(void* ptr)
    {
#if NET6_0_OR_GREATER
        NativeMemory.Free(ptr);
#else
        Marshal.FreeHGlobal((nint)ptr);
#endif
    }

    public static void Free(IntPtr ptr)
    {
#if NET6_0_OR_GREATER
        NativeMemory.Free(ptr.ToPointer());
#else
        Marshal.FreeHGlobal(ptr);
#endif
    }

    public static IntPtr AllocToPointer<T>(T[]? values) where T : unmanaged
    {
        if (values == null
            || values.Length == 0)
        {
            return IntPtr.Zero;
        }

        int structSize = sizeof(T);
        int totalSize = values.Length * structSize;
        IntPtr ptr = Marshal.AllocHGlobal(totalSize);

        byte* walk = (byte*)ptr;
        for (int i = 0; i < values.Length; i++)
        {
            Unsafe.Copy(walk, ref values[i]);
            walk += structSize;
        }

        return ptr;
    }
}
