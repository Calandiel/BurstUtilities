using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Calandiel.Internal
{
	public class Tests : MonoBehaviour
	{
		void Start()
		{
			/*// Tokenizer tests
			List<string> rrr = new List<string>();
			foreach (var item in Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "raw"), "*.txt", SearchOption.AllDirectories))
			{
				Debug.Log(item);
				var str = File.ReadAllText(item);
				var splits = Calandiel.Apocrypha.Tokenizer.Split(str);
				foreach (var obj in splits)
				{
					Debug.Log("-----");
					int ptr = 0;
					while (Calandiel.Apocrypha.Tokenizer.Next(obj, ref ptr, ref rrr))
					{
						Debug.Log("TOKEN");
						foreach (var ar in rrr)
							Debug.Log("---|" + ar);
					}
				}
			}
			//*/
		}

	}
}