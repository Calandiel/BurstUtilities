using UnityEngine;

namespace Calandiel
{
	public class ErrorManager : MonoBehaviour
	{
		public UnityEngine.UI.Text text;


		private static System.Exception ex;
		public static void Throw(System.Exception e)
		{
			ex = e;
			Debug.LogError("(error) Throwing");
			UnityEngine.SceneManagement.SceneManager.LoadScene("ErrorScene");
		}
		public static void Throw(string e) => Throw(new System.Exception(e));
		public static void ThrowWithThrow(string e)
		{
			Throw(e);
			throw new UnityException(e);
		}

		void Start()
		{
			text.text = ex.Message + "\n" + ex.StackTrace;
			Debug.LogError(text.text);
		}

	}
}