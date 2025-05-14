// Decompiled with JetBrains decompiler
// Type: TrashItems.TrashHandler
// Assembly: TrashItems, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8DDB509A-2334-47C1-85BC-EE7C4294997C
// Assembly location: C:\Users\h2so4\Downloads\Trash Items-441-1-2-8-1705314319\TrashItems.dll

using System.Reflection;
using UnityEngine;

#nullable disable
namespace TrashItems;

public class TrashHandler : MonoBehaviour
{
  private UIGroupHandler handler;

  private void Awake()
  {
    this.handler = ((Component) this).GetComponent<UIGroupHandler>();
    this.handler.SetActive(false);
  }

  private void Update()
  {
    if (!ZInput.GetButtonDown("JoyButtonA") || !this.handler.IsActive)
      return;
    TrashItems.TrashItems.TrashItem();
    typeof (InventoryGui).GetMethod("SetActiveGroup", BindingFlags.Instance | BindingFlags.NonPublic).Invoke((object) InventoryGui.instance, new object[2]
    {
      (object) 1,
      (object) false
    });
  }
}
