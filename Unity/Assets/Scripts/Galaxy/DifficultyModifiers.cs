using UnityEngine;
using System.Collections;

public class DifficultyModifier_RandomFluctuation : MonoBehaviour// -cos(0)*0.25 → (beginning at -25%, and oscillating up to +25%)
{
    DungeonMaster.CDifficultyModifier randomFluctuation = new DungeonMaster.CDifficultyModifier(0);

    void Update()
    {
        randomFluctuation.value = -0.25f + Mathf.PingPong(Time.time, 0.5f);
    }
}

public class DifficultyModifier_TotalDistanceTravelled : MonoBehaviour  // +(√km)%
{
    DungeonMaster.CDifficultyModifier totalDistanceTravelled = new DungeonMaster.CDifficultyModifier(0);
    float distanceTravelled = 0.0f;

    void Update()
    {
        distanceTravelled += (CGame.Ship.rigidbody.velocity * Time.deltaTime).magnitude;
        totalDistanceTravelled.value = Mathf.Sqrt(distanceTravelled);
    }
}

public class DifficultyModifier_TotalShipWorth  // Up to 100,000 nanites - +0% to +50%
{
    DungeonMaster.CDifficultyModifier totalShipWorth = new DungeonMaster.CDifficultyModifier(0, 0.2f);
    // Todo: Increase difficulty based on ship worth.
}

public class DifficultyModifier_ShipDamage  // -0% to -50%
{
    DungeonMaster.CDifficultyModifier shipDamage = new DungeonMaster.CDifficultyModifier(-0.25f, 1);
}

public class DifficultyModifier_DifficultyChoice
{
    DungeonMaster.CDifficultyModifier difficultyChoice = new DungeonMaster.CDifficultyModifier(-0.5f, 2);
}