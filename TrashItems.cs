// Decompiled with JetBrains decompiler
// Type: TrashItems.TrashItems
// Assembly: TrashItems, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8DDB509A-2334-47C1-85BC-EE7C4294997C
// Assembly location: C:\Users\h2so4\Downloads\Trash Items-441-1-2-8-1705314319\TrashItems.dll

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace TrashItems;

[BepInPlugin("virtuacode.valheim.trashitems", "Trash Items Mod", "1.2.8")]
internal class TrashItems : BaseUnityPlugin
{
  public static ConfigEntry<bool> ConfirmDialog;
  public static ConfigEntry<KeyboardShortcut> TrashHotkey;
  public static ConfigEntry<TrashItems.TrashItems.SoundEffect> Sfx;
  public static ConfigEntry<Color> TrashColor;
  public static ConfigEntry<string> TrashLabel;
  public static bool _clickedTrash = false;
  public static bool _confirmed = false;
  public static InventoryGui _gui;
  public static Sprite trashSprite;
  public static Sprite bgSprite;
  public static GameObject dialog;
  public static AudioClip[] sounds = new AudioClip[3];
  public static Transform trash;
  public static AudioSource audio;
  public static TrashItems.TrashItems.TrashButton trashButton;
  public static ManualLogSource MyLogger;

  public static void Log(string msg) => TrashItems.TrashItems.MyLogger?.LogInfo((object) msg);

  public static void LogErr(string msg) => TrashItems.TrashItems.MyLogger?.LogError((object) msg);

  public static void LogWarn(string msg) => TrashItems.TrashItems.MyLogger?.LogWarning((object) msg);

  private void Awake()
  {
    TrashItems.TrashItems.MyLogger = this.Logger;
    TrashItems.TrashItems.ConfirmDialog = this.Config.Bind<bool>("General", "ConfirmDialog", false, "Show confirm dialog");
    TrashItems.TrashItems.Sfx = this.Config.Bind<TrashItems.TrashItems.SoundEffect>("General", "SoundEffect", TrashItems.TrashItems.SoundEffect.Random, "Sound effect when trashing items");
    TrashItems.TrashItems.TrashHotkey = this.Config.Bind<KeyboardShortcut>("Input", "TrashHotkey", KeyboardShortcut.Deserialize("Delete"), "Hotkey for destroying items");
    TrashItems.TrashItems.TrashLabel = this.Config.Bind<string>("General", "TrashLabel", "Trash", "Label for the trash button");
    TrashItems.TrashItems.TrashColor = this.Config.Bind<Color>("General", "TrashColor", new Color(1f, 0.8482759f, 0.0f), "Color for the trash label");
    TrashItems.TrashItems.TrashLabel.SettingChanged += (EventHandler) ((sender, args) =>
    {
      if (!Object.op_Inequality((Object) TrashItems.TrashItems.trashButton, (Object) null))
        return;
      TrashItems.TrashItems.trashButton.SetText(TrashItems.TrashItems.TrashLabel.Value);
    });
    TrashItems.TrashItems.TrashColor.SettingChanged += (EventHandler) ((sender, args) =>
    {
      if (!Object.op_Inequality((Object) TrashItems.TrashItems.trashButton, (Object) null))
        return;
      TrashItems.TrashItems.trashButton.SetColor(TrashItems.TrashItems.TrashColor.Value);
    });
    TrashItems.TrashItems.Log("TrashItems Loaded!");
    TrashItems.TrashItems.trashSprite = TrashItems.TrashItems.LoadSprite("TrashItems.res.trash.png", new Rect(0.0f, 0.0f, 64f, 64f), new Vector2(32f, 32f));
    TrashItems.TrashItems.bgSprite = TrashItems.TrashItems.LoadSprite("TrashItems.res.trashmask.png", new Rect(0.0f, 0.0f, 96f, 112f), new Vector2(48f, 56f));
    TrashItems.TrashItems.sounds[0] = TrashItems.TrashItems.LoadAudioClip("TrashItems.res.trash1.wav");
    TrashItems.TrashItems.sounds[1] = TrashItems.TrashItems.LoadAudioClip("TrashItems.res.trash2.wav");
    TrashItems.TrashItems.sounds[2] = TrashItems.TrashItems.LoadAudioClip("TrashItems.res.trash3.wav");
    Harmony.CreateAndPatchAll(typeof (TrashItems.TrashItems), (string) null);
  }

  public static AudioClip LoadAudioClip(string path)
  {
    using (MemoryStream destination = new MemoryStream())
    {
      Assembly.GetExecutingAssembly().GetManifestResourceStream(path).CopyTo((Stream) destination);
      return WavUtility.ToAudioClip(destination.GetBuffer());
    }
  }

  public static Sprite LoadSprite(string path, Rect size, Vector2 pivot, int units = 100)
  {
    Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
    Texture2D texture2D = new Texture2D((int) ((Rect) ref size).width, (int) ((Rect) ref size).height, (TextureFormat) 4, false, true);
    using (MemoryStream destination = new MemoryStream())
    {
      manifestResourceStream.CopyTo((Stream) destination);
      ImageConversion.LoadImage(texture2D, destination.ToArray());
      texture2D.Apply();
      return Sprite.Create(texture2D, size, pivot, (float) units);
    }
  }

  [HarmonyPatch(typeof (InventoryGui), "Show")]
  [HarmonyPostfix]
  public static void Show_Postfix(InventoryGui __instance)
  {
    Transform transform = ((Component) InventoryGui.instance.m_player).transform;
    TrashItems.TrashItems.trash = transform.Find("Trash");
    if (Object.op_Inequality((Object) TrashItems.TrashItems.trash, (Object) null))
      return;
    TrashItems.TrashItems._gui = InventoryGui.instance;
    TrashItems.TrashItems.trash = Object.Instantiate<Transform>(transform.Find("Armor"), transform);
    TrashItems.TrashItems.trashButton = ((Component) TrashItems.TrashItems.trash).gameObject.AddComponent<TrashItems.TrashItems.TrashButton>();
    AudioMixerGroup matchingGroup = AudioMan.instance.m_masterMixer.FindMatchingGroups("GUI")[0];
    TrashItems.TrashItems.audio = ((Component) TrashItems.TrashItems.trash).gameObject.AddComponent<AudioSource>();
    TrashItems.TrashItems.audio.playOnAwake = false;
    TrashItems.TrashItems.audio.loop = false;
    TrashItems.TrashItems.audio.outputAudioMixerGroup = matchingGroup;
    TrashItems.TrashItems.audio.bypassReverbZones = true;
  }

  [HarmonyPatch(typeof (InventoryGui), "Hide")]
  [HarmonyPostfix]
  public static void Postfix() => TrashItems.TrashItems.OnCancel();

  [HarmonyPostfix]
  [HarmonyPatch(typeof (InventoryGui), "UpdateItemDrag")]
  public static void UpdateItemDrag_Postfix(
    InventoryGui __instance,
    ref GameObject ___m_dragGo,
    ItemDrop.ItemData ___m_dragItem,
    Inventory ___m_dragInventory,
    int ___m_dragAmount)
  {
    KeyboardShortcut keyboardShortcut = TrashItems.TrashItems.TrashHotkey.Value;
    if (((KeyboardShortcut) ref keyboardShortcut).IsDown())
      TrashItems.TrashItems._clickedTrash = true;
    if (TrashItems.TrashItems._clickedTrash && ___m_dragItem != null && ___m_dragInventory.ContainsItem(___m_dragItem))
    {
      if (TrashItems.TrashItems.ConfirmDialog.Value)
      {
        if (TrashItems.TrashItems._confirmed)
        {
          TrashItems.TrashItems._confirmed = false;
        }
        else
        {
          TrashItems.TrashItems.ShowConfirmDialog(___m_dragItem, ___m_dragAmount);
          TrashItems.TrashItems._clickedTrash = false;
          return;
        }
      }
      if (___m_dragAmount == ___m_dragItem.m_stack)
      {
        ((Humanoid) Player.m_localPlayer).RemoveEquipAction(___m_dragItem);
        ((Humanoid) Player.m_localPlayer).UnequipItem(___m_dragItem, false);
        ___m_dragInventory.RemoveItem(___m_dragItem);
      }
      else
        ___m_dragInventory.RemoveItem(___m_dragItem, ___m_dragAmount);
      if (Object.op_Inequality((Object) TrashItems.TrashItems.audio, (Object) null))
      {
        switch (TrashItems.TrashItems.Sfx.Value)
        {
          case TrashItems.TrashItems.SoundEffect.Sound1:
            TrashItems.TrashItems.audio.PlayOneShot(TrashItems.TrashItems.sounds[0]);
            break;
          case TrashItems.TrashItems.SoundEffect.Sound2:
            TrashItems.TrashItems.audio.PlayOneShot(TrashItems.TrashItems.sounds[1]);
            break;
          case TrashItems.TrashItems.SoundEffect.Sound3:
            TrashItems.TrashItems.audio.PlayOneShot(TrashItems.TrashItems.sounds[2]);
            break;
          case TrashItems.TrashItems.SoundEffect.Random:
            TrashItems.TrashItems.audio.PlayOneShot(TrashItems.TrashItems.sounds[Random.Range(0, 3)]);
            break;
        }
      }
      __instance.GetType().GetMethod("SetupDragItem", BindingFlags.Instance | BindingFlags.NonPublic).Invoke((object) __instance, new object[3]
      {
        null,
        null,
        (object) 0
      });
      __instance.GetType().GetMethod("UpdateCraftingPanel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke((object) __instance, new object[1]
      {
        (object) false
      });
    }
    TrashItems.TrashItems._clickedTrash = false;
  }

  public static void ShowConfirmDialog(ItemDrop.ItemData item, int itemAmount)
  {
    if (Object.op_Equality((Object) InventoryGui.instance, (Object) null) || Object.op_Inequality((Object) TrashItems.TrashItems.dialog, (Object) null))
      return;
    TrashItems.TrashItems.dialog = Object.Instantiate<GameObject>(((Component) InventoryGui.instance.m_splitPanel).gameObject, ((Component) InventoryGui.instance).transform);
    Button component1 = ((Component) TrashItems.TrashItems.dialog.transform.Find("win_bkg/Button_ok")).GetComponent<Button>();
    ((UnityEventBase) component1.onClick).RemoveAllListeners();
    // ISSUE: method pointer
    ((UnityEvent) component1.onClick).AddListener(new UnityAction((object) null, __methodptr(OnConfirm)));
    ((TMP_Text) ((Component) component1).GetComponentInChildren<TextMeshProUGUI>()).text = "Trash";
    ((Graphic) ((Component) component1).GetComponentInChildren<TextMeshProUGUI>()).color = new Color(1f, 0.2f, 0.1f);
    Button component2 = ((Component) TrashItems.TrashItems.dialog.transform.Find("win_bkg/Button_cancel")).GetComponent<Button>();
    ((UnityEventBase) component2.onClick).RemoveAllListeners();
    // ISSUE: method pointer
    ((UnityEvent) component2.onClick).AddListener(new UnityAction((object) null, __methodptr(OnCancel)));
    ((Component) TrashItems.TrashItems.dialog.transform.Find("win_bkg/Slider")).gameObject.SetActive(false);
    ((TMP_Text) ((Component) TrashItems.TrashItems.dialog.transform.Find("win_bkg/Text")).GetComponent<TextMeshProUGUI>()).text = Localization.instance.Localize(item.m_shared.m_name);
    ((Component) TrashItems.TrashItems.dialog.transform.Find("win_bkg/Icon_bkg/Icon")).GetComponent<Image>().sprite = item.GetIcon();
    ((TMP_Text) ((Component) TrashItems.TrashItems.dialog.transform.Find("win_bkg/amount")).GetComponent<TextMeshProUGUI>()).text = $"{itemAmount.ToString()}/{item.m_shared.m_maxStackSize.ToString()}";
    TrashItems.TrashItems.dialog.gameObject.SetActive(true);
  }

  public static void OnConfirm()
  {
    TrashItems.TrashItems._confirmed = true;
    if (Object.op_Inequality((Object) TrashItems.TrashItems.dialog, (Object) null))
    {
      Object.Destroy((Object) TrashItems.TrashItems.dialog);
      TrashItems.TrashItems.dialog = (GameObject) null;
    }
    TrashItems.TrashItems.TrashItem();
  }

  public static void OnCancel()
  {
    TrashItems.TrashItems._confirmed = false;
    if (!Object.op_Inequality((Object) TrashItems.TrashItems.dialog, (Object) null))
      return;
    Object.Destroy((Object) TrashItems.TrashItems.dialog);
    TrashItems.TrashItems.dialog = (GameObject) null;
  }

  public static void TrashItem()
  {
    TrashItems.TrashItems.Log("Trash Items clicked!");
    if (Object.op_Equality((Object) TrashItems.TrashItems._gui, (Object) null))
    {
      TrashItems.TrashItems.LogErr("_gui is null");
    }
    else
    {
      TrashItems.TrashItems._clickedTrash = true;
      TrashItems.TrashItems._gui.GetType().GetMethod("UpdateItemDrag", BindingFlags.Instance | BindingFlags.NonPublic).Invoke((object) TrashItems.TrashItems._gui, new object[0]);
    }
  }

  public enum SoundEffect
  {
    None,
    Sound1,
    Sound2,
    Sound3,
    Random,
  }

  public class TrashButton : MonoBehaviour
  {
    private Canvas canvas;
    private GraphicRaycaster raycaster;
    private RectTransform rectTransform;
    private GameObject buttonGo;

    private void Awake()
    {
      if (Object.op_Equality((Object) InventoryGui.instance, (Object) null))
        return;
      Transform transform1 = ((Component) InventoryGui.instance.m_player).transform;
      RectTransform component = ((Component) this).GetComponent<RectTransform>();
      component.anchoredPosition = Vector2.op_Subtraction(component.anchoredPosition, new Vector2(0.0f, 78f));
      this.SetText(TrashItems.TrashItems.TrashLabel.Value);
      this.SetColor(TrashItems.TrashItems.TrashColor.Value);
      Transform transform2 = ((Component) this).transform.Find("armor_icon");
      if (!Object.op_Implicit((Object) transform2))
        TrashItems.TrashItems.LogErr("armor_icon not found!");
      ((Component) transform2).GetComponent<Image>().sprite = TrashItems.TrashItems.trashSprite;
      ((Component) this).transform.SetSiblingIndex(0);
      ((Object) ((Component) ((Component) this).transform).gameObject).name = "Trash";
      this.buttonGo = new GameObject("ButtonCanvas");
      this.rectTransform = this.buttonGo.AddComponent<RectTransform>();
      ((Component) this.rectTransform).transform.SetParent(((Component) ((Component) this).transform).transform, true);
      this.rectTransform.anchoredPosition = Vector2.zero;
      this.rectTransform.sizeDelta = new Vector2(70f, 74f);
      this.canvas = this.buttonGo.AddComponent<Canvas>();
      this.raycaster = this.buttonGo.AddComponent<GraphicRaycaster>();
      // ISSUE: method pointer
      ((UnityEvent) this.buttonGo.AddComponent<Button>().onClick).AddListener(new UnityAction((object) null, __methodptr(TrashItem)));
      ((Graphic) this.buttonGo.AddComponent<Image>()).color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
      GameObject gameObject = Object.Instantiate<GameObject>(((Component) transform1.Find("selected_frame").GetChild(0)).gameObject, ((Component) this).transform);
      gameObject.GetComponent<Image>().sprite = TrashItems.TrashItems.bgSprite;
      gameObject.transform.SetAsFirstSibling();
      gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(-8f, 22f);
      gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(6f, 7.5f);
      UIGroupHandler uiGroupHandler = ((Component) this).gameObject.AddComponent<UIGroupHandler>();
      uiGroupHandler.m_groupPriority = 1;
      uiGroupHandler.m_enableWhenActiveAndGamepad = gameObject;
      TrashItems.TrashItems._gui.m_uiGroups = CollectionExtensions.AddToArray<UIGroupHandler>(TrashItems.TrashItems._gui.m_uiGroups, uiGroupHandler);
      ((Component) this).gameObject.AddComponent<TrashHandler>();
    }

    private void Start() => this.StartCoroutine(this.DelayedOverrideSorting());

    private IEnumerator DelayedOverrideSorting()
    {
      yield return (object) null;
      if (!Object.op_Equality((Object) this.canvas, (Object) null))
      {
        this.canvas.overrideSorting = true;
        this.canvas.sortingOrder = 1;
      }
    }

    public void SetText(string text)
    {
      Transform transform = ((Component) this).transform.Find("ac_text");
      if (!Object.op_Implicit((Object) transform))
        TrashItems.TrashItems.LogErr("ac_text not found!");
      else
        ((Component) transform).GetComponent<TMP_Text>().text = text;
    }

    public void SetColor(Color color)
    {
      Transform transform = ((Component) this).transform.Find("ac_text");
      if (!Object.op_Implicit((Object) transform))
        TrashItems.TrashItems.LogErr("ac_text not found!");
      else
        ((Graphic) ((Component) transform).GetComponent<TMP_Text>()).color = color;
    }
  }
}
