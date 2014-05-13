using System;
using UnityEngine;
using System.Collections;
public class MyGui : MonoBehaviour
{
  public Light DirLight;
  public GameObject Target;
  public GameObject BallPosition;
  public GameObject BuffPosition;
  public GameObject OtherSkillsPosition;
  public GameObject Plane1;
  public GameObject Plane2;
  public Material[] PlaneMaterials;
  public GameObject[] Prefabs;

  private float oldLightIntensity;
  private Color oldAmbientColor;
  private GameObject currentGo, currentBall;
  private bool isDay, isHomingMove, isDefaultPlaneTexture;
  private int current;
  private Animator anim;
  private float prefabSpeed = 4, oldPrefabSpeed;

  private GUIStyle guiStyleHeader = new GUIStyle();

  void Start()
  {
    oldAmbientColor = RenderSettings.ambientLight;
    oldLightIntensity = DirLight.intensity;

    anim = Target.GetComponent<Animator>();
    guiStyleHeader.fontSize = 14;
    guiStyleHeader.normal.textColor = new Color(1,1,1);
    InvokeRepeating("InstanceBall", 2, 2);
    InstanceCurrentBall();
  }

  private void InstanceBall()
  {
    var temp = Instantiate(Prefabs[current], transform.position, Prefabs[current].transform.rotation) as GameObject;
    var prefabSettings = temp.GetComponent<PrefabSettings>();
    prefabSettings.Target = Target;
    if (isHomingMove)
      prefabSettings.IsHomingMove = isHomingMove;
    prefabSettings.MoveSpeed = prefabSpeed;
  }

  private void InstanceShot()
  {
    var temp = Instantiate(Prefabs[current], transform.position, Prefabs[current].transform.rotation) as GameObject;
    var prefabSettings = temp.GetComponent<PrefabSettings>();
    prefabSettings.Target = Target;
  }

  private void InstanceCurrentBall()
  {
    currentBall = Instantiate(Prefabs[current], BallPosition.transform.position, Prefabs[current].transform.rotation) as GameObject;
    var prefabSettings = currentBall.GetComponent<PrefabSettings>();
    prefabSettings.Target = Target;
    prefabSettings.PrefabStatus = PrefabStatus.FadeIn;
  }

  private void InstanceBuff()
  {
    currentGo = Instantiate(Prefabs[current], BuffPosition.transform.position, Prefabs[current].transform.rotation) as GameObject;
  }

  private void InstanceOther()
  {
    currentGo = Instantiate(Prefabs[current], OtherSkillsPosition.transform.position, Prefabs[current].transform.rotation) as GameObject;
  }

  private void InstancePrefabForBuffs()
  {
    var temp = Instantiate(Prefabs[1], transform.position, Prefabs[current].transform.rotation) as GameObject;
    temp.GetComponent<PrefabSettings>().Target = Target;
  }
  private void OnGUI()
  {
    if (GUI.Button(new Rect(10, 15, 105, 30), "Previous Effect")) {
      ChangeCurrent(-1);
    }
    if (GUI.Button(new Rect(130, 15, 105, 30), "Next Effect"))
    {
      ChangeCurrent(+1);
    }
    GUI.Label(new Rect(300, 15, 100, 20), "Prefab name is \"" + Prefabs[current].name + "\"  \r\nHold any mouse button that would move the camera", guiStyleHeader);
    if (GUI.Button(new Rect(10, 60, 225, 30), "Day/Night")) {
      DirLight.intensity = !isDay ? 0.00f : oldLightIntensity;
      RenderSettings.ambientLight = !isDay ? new Color(0.1f, 0.1f, 0.1f) : oldAmbientColor;
      isDay = !isDay;
    }
    if (GUI.Button(new Rect(10, 105, 225, 30), "Change environment")) {
      if (isDefaultPlaneTexture) {
        Plane1.renderer.material = PlaneMaterials[0];
        Plane2.renderer.material = PlaneMaterials[0];
      }
      else {
        Plane1.renderer.material = PlaneMaterials[1];
        Plane2.renderer.material = PlaneMaterials[2];
      }
      isDefaultPlaneTexture = !isDefaultPlaneTexture;
    }
    if (current >= 0 && current <= 12) {
      GUI.Label(new Rect(10, 152, 225, 30), "Ball Speed "  + (int)prefabSpeed + "m", guiStyleHeader);
      prefabSpeed = GUI.HorizontalSlider(new Rect(115, 155, 120, 30), prefabSpeed, 1.0F, 30.0F);
      isHomingMove = GUI.Toggle(new Rect(10, 190, 150, 30), isHomingMove, " Is Homing Move");
    }

  }

  void Update()
  {
    anim.enabled = isHomingMove;
  }

  void ChangeCurrent(int delta)
  {
    Destroy(currentGo);
    Destroy(currentBall);
    BuffPosition.SetActive(false);
    current += delta;
    if (current> Prefabs.Length - 1)
      current = 0;
    else if (current < 0)
      current = Prefabs.Length - 1;
      
    CancelInvoke("InstanceBall");
    CancelInvoke("InstanceShot");
    CancelInvoke("InstanceBuff");
    CancelInvoke("InstanceOther");
    CancelInvoke("InstancePrefabForBuffs");
    if (current >= 0 && current <= 12) {
      prefabSpeed = 4;
      InvokeRepeating("InstanceBall", 2, 2);
      InstanceCurrentBall();
    }
    else if (current >= 13 && current <= 15)
    {
      InvokeRepeating("InstanceShot", 0, 2);
    }
    else if (current >= 18 && current <= 27) {
      prefabSpeed = 4;
      BuffPosition.SetActive(true);
      InvokeRepeating("InstancePrefabForBuffs", 3, 3);
      InstanceBuff();
    }
    else {
      InstanceOther();
    }
  }
}