using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Calandiel.Internal
{
	public static class Hash
	{
		public static uint pcg_hash(uint input)
		{
			unchecked
			{
				uint state = input * 747796405u + 2891336453u;
				uint word = ((state >> (int)((state >> 28) + 4u)) ^ state) * 277803737u;
				return (word >> 22) ^ word;
			}
		}
	}
}