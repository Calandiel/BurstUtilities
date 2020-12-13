namespace Calandiel.Utilities
{
	/// <summary>
	/// This class is meant to be placed on Button UI MonoBehaviours.
	/// Refer to the OnClick method to quickly create buttons that close the game.
	/// </summary>
	public class QuitUtility : UnityEngine.MonoBehaviour
	{
		public void OnClick()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			UnityEngine.Application.Quit();
#endif
		}
	}
}
