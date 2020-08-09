using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShaderNoise {

	const int size = 256;

	static readonly int[] source = new int[] {
		151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142,
		8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203,
		117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165,
		71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41,
		55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89,
		18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250,
		124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189,
		28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
		129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34,
		242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31,
		181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114,
		67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
	};


	public static void SetupNoise(ComputeShader shader, int seed) {

		// Apply seed.
		int[] random = Randomize(seed);
		shader.SetInts("noise_random", random);

	}


	static int[] Randomize(int seed) {

		int[] random = new int[size * 2];

		if(seed != 0) {
			
			// Shuffle the array using the given seed
			// Unpack the seed into 4 bytes then perform a bitwise XOR operation
			// with each byte
			var F = new byte[4];
			UnpackLittleUint32(seed, ref F);

			for(int i = 0; i < source.Length; i++) {

				random[i] = source[i];
				random[i] ^= F[0];
				random[i] ^= F[1];
				random[i] ^= F[2];
				random[i] ^= F[3];

				random[i + size] = random[i];

			}

		} else {

			for(int i = 0; i < size; i++) {
				random[i + size] = random[i] = source[i];
			}

		}

		return random;

	}

	/// <summary>
	/// Unpack the given integer (int32) to an array of 4 bytes  in little endian format.
	/// If the length of the buffer is too smal, it wil be resized.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="buffer">The output buffer.</param>
	static byte[] UnpackLittleUint32(int value, ref byte[] buffer) {
		
		if(buffer.Length < 4) Array.Resize(ref buffer, 4);

		buffer[0] = (byte)((value & 0x000000ff) >> 0);
		buffer[1] = (byte)((value & 0x0000ff00) >> 8);
		buffer[2] = (byte)((value & 0x00ff0000) >> 16);
		buffer[3] = (byte)((value & 0xff000000) >> 24);

		return buffer;
	}

}
