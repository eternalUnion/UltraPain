using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Ultrapain
{
	public static class StyleManager
	{
		private static bool registered = false;
		public static void RegisterIDs()
		{
			registered = false;
			if (MonoSingleton<StyleHUD>.Instance == null)
				return;

			MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(ConfigManager.grenadeBoostStyleText.guid, ConfigManager.grenadeBoostStyleText.formattedString);
			MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(ConfigManager.rocketBoostStyleText.guid, ConfigManager.rocketBoostStyleText.formattedString);

			MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(ConfigManager.orbStrikeRevolverStyleText.guid, ConfigManager.orbStrikeRevolverStyleText.formattedString);
			MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(ConfigManager.orbStrikeRevolverChargedStyleText.guid, ConfigManager.orbStrikeRevolverChargedStyleText.formattedString);
			MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(ConfigManager.orbStrikeElectricCannonStyleText.guid, ConfigManager.orbStrikeElectricCannonStyleText.formattedString);
			MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(ConfigManager.orbStrikeMaliciousCannonStyleText.guid, ConfigManager.orbStrikeMaliciousCannonStyleText.formattedString);
			MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(ConfigManager.maliciousChargebackStyleText.guid, ConfigManager.maliciousChargebackStyleText.formattedString);
			MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(ConfigManager.sentryChargebackStyleText.guid, ConfigManager.sentryChargebackStyleText.formattedString);

			registered = true;
			Debug.Log("Registered all style ids");
		}

		public static void UpdateID(string id, string newName)
		{
			if (!registered || StyleHUD.Instance == null)
				return;
			StyleHUD.Instance.idNameDict[id] = newName;
		}
	}
}
