using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SteamFriendDisplay : MonoBehaviour
{
     public TextMeshProUGUI displayNameText;
     public RawImage profilePicture;

     public void SetDisplayInfo(string displayName, RawImage image)
     {
          displayNameText.text = displayName;
          profilePicture.texture = image.texture;
     }
}