using UnityEngine;
using System.Collections;

public class DifficultyModifier_RandomFluctuation : MonoBehaviour// -cos(0)*0.25 → (beginning at -25%, and oscillating up to +25%)
{
    DungeonMaster.DifficultyModifier randomFluctuation = new DungeonMaster.DifficultyModifier(0);

    void Update()
    {
        randomFluctuation.value = -0.25f + Mathf.PingPong(Time.time*0.025f, 0.5f);
    }
}

public class DifficultyModifier_TotalDistanceTravelled : MonoBehaviour  // +(√km)%
{
    DungeonMaster.DifficultyModifier totalDistanceTravelled = new DungeonMaster.DifficultyModifier(0);
    float distanceTravelled = 0.0f;

    void Update()
    {
        distanceTravelled += (CGameShips.GalaxyShip.rigidbody.velocity * Time.deltaTime).magnitude;
        totalDistanceTravelled.value = Mathf.Sqrt(distanceTravelled) * 0.005f;
    }
}

public class DifficultyModifier_TotalShipWorth  // Up to 100,000 nanites - +0% to +50%
{
    DungeonMaster.DifficultyModifier totalShipWorth = new DungeonMaster.DifficultyModifier(0, 0.2f);
    // Todo: Increase difficulty based on ship worth.
}

public class DifficultyModifier_ShipDamage  // -0% to -50%
{
    DungeonMaster.DifficultyModifier shipDamage = new DungeonMaster.DifficultyModifier(-0.25f, 1);
}

public class DifficultyModifier_DifficultyChoice    // Sandbox (-100%), Casual (-75%), Easy (-50%), medium (0%), hard (+50%), insane (+100%)
{
    DungeonMaster.DifficultyModifier difficultyChoice = new DungeonMaster.DifficultyModifier(-0.5f, 2);
}