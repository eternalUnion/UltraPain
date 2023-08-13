using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltrapainExtensions
{
	public class NonUltrapainEnemy : MonoBehaviour
	{
		[Tooltip("If enabled, ultrapain stats editor will not modify the speed, health and damage modifiers of the enemy based on the enemy identifier type")]
		public bool disableStatEditor = false;
		[Tooltip("If enabled, ultrapain resistance editor will not add additional weakneses based on the enemy identifier type")]
		public bool disableResistanceEditor = false;
	}
}
