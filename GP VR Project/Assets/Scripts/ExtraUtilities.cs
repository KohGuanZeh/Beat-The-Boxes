using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Example Scripts
public class LambdaExpressionExample
{
	public delegate bool LamdaExpressionDel(string name, float val);
	public LamdaExpressionDel LambdaExpressionHandler;

	public void AddFunctions()
	{

	}

	public bool LambdaExpressionOne(string name, float val)
	{
		if (name != string.Empty && val > 0) return true;
		else return false;
	}

	public bool LambdaExperienceTwo(int index, string name, float val)
	{
		if (index > 0 && name != string.Empty && val > 0) return true;
		else return false;
	}
}
#endregion

//Developed by Koh Guan Zeh
namespace XellExtraUtils
{
	public static class MathFunctions
	{
		public static float SmoothPingPong(float time, float maxOffset = 1, float speed = 1, float startOffset = 0, bool minIsStartNumber = false)
		{
			return minIsStartNumber ? maxOffset * Mathf.Sin(speed * Mathf.PI * time) + startOffset + maxOffset : maxOffset * Mathf.Sin(speed * Mathf.PI * time) + startOffset;
		}

		public static int RandomisePositiveNegative()
		{
			/*System.Random rdm = new System.Random();
			return rdm.Next(0, 100) >= 50 ? 1 : -1;*/
			return Random.Range(0, 100) >= 50 ? 1 : -1;
		}

		//These Lerp Functions are Learnt from Chico - How to Lerp Like a Pro!
		#region Float Lerp Functions
		public static float Sinerp(float a, float b, float t)
		{
			t = Mathf.Sin(t * Mathf.PI * 0.5f);
			return Mathf.Lerp(a, b, t);
		}

		public static float Coserp(float a, float b, float t)
		{
			t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
			return Mathf.Lerp(a, b, t);
		}

		public static float ExponentialLerp(float a, float b, float t)
		{
			t = t * t;
			return Mathf.Lerp(a, b, t);
		}

		public static float SmoothStepLerp(float a, float b, float t)
		{
			t = t * t * (3f - 2f * t);
			return Mathf.Lerp(a, b, t);
		}

		public static float SmootherStepLerp(float a, float b, float t)
		{
			t = t * t * t * (t * (6f * t - 15f) + 10f);
			return Mathf.Lerp(a, b, t);
		}
		#endregion

		#region Vector2 Lerp Functions
		public static Vector2 Sinerp(Vector2 a, Vector2 b, float t)
		{
			t = Mathf.Sin(t * Mathf.PI * 0.5f);
			return Vector2.Lerp(a, b, t);
		}

		public static Vector2 Coserp(Vector2 a, Vector2 b, float t)
		{
			t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
			return Vector2.Lerp(a, b, t);
		}

		public static Vector2 ExponentialLerp(Vector2 a, Vector2 b, float t)
		{
			t = t * t;
			return Vector2.Lerp(a, b, t);
		}

		public static Vector2 SmoothStepLerp(Vector2 a, Vector2 b, float t)
		{
			t = t * t * (3f - 2f * t);
			return Vector2.Lerp(a, b, t);
		}

		public static Vector2 SmootherStepLerp(Vector2 a, Vector2 b, float t)
		{
			t = t * t * t * (t * (6f * t - 15f) + 10f);
			return Vector2.Lerp(a, b, t);
		}
		#endregion

		#region Vector3 Lerp Functions
		public static Vector3 Sinerp(Vector3 a, Vector3 b, float t)
		{
			t = Mathf.Sin(t * Mathf.PI * 0.5f);
			return Vector3.Lerp(a, b, t);
		}

		public static Vector3 Coserp(Vector3 a, Vector3 b, float t)
		{
			t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
			return Vector3.Lerp(a, b, t);
		}

		public static Vector3 ExponentialLerp(Vector3 a, Vector3 b, float t)
		{
			t = t * t;
			return Vector3.Lerp(a, b, t);
		}

		public static Vector3 SmoothStepLerp(Vector3 a, Vector3 b, float t)
		{
			t = t * t * (3f - 2f * t);
			return Vector3.Lerp(a, b, t);
		}

		public static Vector3 SmootherStepLerp(Vector3 a, Vector3 b, float t)
		{
			t = t * t * t * (t * (6f * t - 15f) + 10f);
			return Vector3.Lerp(a, b, t);
		}
		#endregion

		#region Color Lerp Functions
		public static Color Sinerp(Color a, Color b, float t)
		{
			t = Mathf.Sin(t * Mathf.PI * 0.5f);
			return Color.Lerp(a, b, t);
		}

		public static Color Coserp(Color a, Color b, float t)
		{
			t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
			return Color.Lerp(a, b, t);
		}

		public static Color ExponentialLerp(Color a, Color b, float t)
		{
			t = t * t;
			return Color.Lerp(a, b, t);
		}

		public static Color SmoothStepLerp(Color a, Color b, float t)
		{
			t = t * t * (3f - 2f * t);
			return Color.Lerp(a, b, t);
		}

		public static Color SmootherStepLerp(Color a, Color b, float t)
		{
			t = t * t * t * (t * (6f * t - 15f) + 10f);
			return Color.Lerp(a, b, t);
		}
		#endregion
	}

	public static class MaterialUtils
	{
		public static Material[] GetMaterialsFromRenderers(Renderer[] rs)
		{
			Material[] mats = new Material[rs.Length];
			for (int i = 0; i < mats.Length; i++) mats[i] = rs[i].material;

			return mats;
		}

		//Not Tested
		public static Material[] GetMaterialsFromRenderers(Renderer[] rs, int matIndex)
		{
			Material[] mats = new Material[rs.Length];
			for (int i = 0; i < mats.Length; i++) mats[i] = rs[i].materials[matIndex];

			return mats;
		}

		public static void ChangeMaterialColor(Material mat, Color color, string colorProperty = "_BaseColor")
		{
			mat.SetColor(colorProperty, color);
		}

		public static void ChangeMaterialsColor(Material[] mats, Color color, string colorProperty = "_BaseColor")
		{
			foreach (Material mat in mats) mat.SetColor(colorProperty, color);
		}

		/// <summary>
		/// Toggles the Emission Property for a Selected Material.
		/// Emission must always be turned on at the Start of the Game for the Enable Keyword to work.
		/// This is because Unity will leave out the Emission Property when Exporting the Material when it is not registered as being used.
		/// </summary>
		/// <param name="mat">Material To Toggle Emission</param>
		/// <param name="emissionOn">Should Emission be On?</param>
		/// <param name="emissionProperty">Shader Keyword for Emission</param>
		public static void ToggleMaterialEmission(Material mat, bool emissionOn, string emissionProperty = "_EMISSION") //Emission must always be turned on first for Enable Keyword to work
		{
			if (emissionOn) mat.EnableKeyword(emissionProperty);
			else mat.DisableKeyword(emissionProperty);
		}

		/// <summary>
		/// Toggles the Emission Property for the Selected Materials.
		/// Emission must always be turned on at the Start of the Game for the Enable Keyword to work.
		/// This is because Unity will leave out the Emission Property when Exporting the Material when it is not registered as being used.
		/// </summary>
		/// <param name="mats">Materials To Toggle Emission</param>
		/// <param name="emissionOn">Should Emission be On?</param>
		/// <param name="emissionProperty">Shader Keyword for Emission</param>
		public static void ToggleMaterialsEmission(Material[] mats, bool emissionOn, string emissionProperty = "_EMISSION") //Emission must always be turned on first for Enable Keyword to work
		{
			foreach (Material mat in mats)
			{
				if (emissionOn) mat.EnableKeyword(emissionProperty);
				else mat.DisableKeyword(emissionProperty);
			}
		}

		/// <summary>
		/// Changes the Selected Material's Emission Color
		/// Emission must always be turned on at the Start of the Game for the Enable Keyword to work.
		/// This is because Unity will leave out the Emission Property when Exporting the Material when it is not registered as being used.
		/// </summary>
		/// <param name="mat">Material requiring Emissive Color Change</param>
		/// <param name="color">Color of the Emission</param>
		/// <param name="intensity">Intensity of the Emission</param>
		/// <param name="emissionProperty">Shader Keyword for Emission</param>
		public static void ChangeMaterialEmission(Material mat, Color color, float intensity, string emissionProperty = "_EmissionColor")
		{
			//HDRP uses _EmissiveColor and is also Color * Intensity (Luminance)
			mat.SetColor(emissionProperty, color * intensity);
		}

		/// <summary>
		/// Changes the Selected Material's Emission Color
		/// Emission must always be turned on at the Start of the Game for the Enable Keyword to work.
		/// This is because Unity will leave out the Emission Property when Exporting the Material when it is not registered as being used.
		/// </summary>
		/// <param name="mats">Materials that requires Emissive Color Change</param>
		/// <param name="color">Color of the Emission</param>
		/// <param name="intensity">Intensity of the Emission</param>
		/// <param name="emissionProperty">Shader Keyword for Emission</param>
		public static void ChangeMaterialsEmission(Material[] mats, Color color, float intensity, string emissionProperty = "_EmissionColor")
		{
			//HDRP uses _EmissiveColor and is also Color * Intensity (Luminance)
			foreach (Material mat in mats) mat.SetColor(emissionProperty, color * intensity);
		}
	}

}
