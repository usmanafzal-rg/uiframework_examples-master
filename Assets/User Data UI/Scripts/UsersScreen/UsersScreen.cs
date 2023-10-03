using System;
using System.Collections;
using System.Collections.Generic;
using deVoid.UIFramework;
using deVoid.Utils;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = System.Object;

[Serializable]
public class Name
{
    public string title = "";
    public string first = "";
    public string last = "";
}

[Serializable]
public class DOB
{
    public string date;
    public int age;
}

[Serializable]
public class Picture
{
    public string large;
    public string medium;
    public string thumbnail;
}

[Serializable]
public class Result
{
    public string gender = "";
    public Name name;
    public string email = "";
    public DOB dob;
    public string phone;
    public Picture picture;
}
[Serializable]
public class Info
{
    public string seed = "";
    public int results;
    public int page;
    public string version = "";
}
[Serializable]
public class UsersData
{
    public List<Result> results;
    public Info info;
}
[Serializable]
public class UserData : WindowProperties
{
    public string first;
    public string last;
    public string email;
    public string gender;
    public string phone;
    public int age;
    public string imageUrl;
    public Sprite image = null;
    public string thumbnailUrl = null;
    public UserData(string fname, string lname, string email, string gender, string phone, int age, string imageUrl, string thumbnailUrl)
    {
        first = fname;
        last = lname;
        this.email = email;
        this.gender = gender;
        this.phone = phone;
        this.age = age;
        this.imageUrl = imageUrl;
        this.thumbnailUrl = thumbnailUrl;
    }
}
[Serializable]
public class UsersScreenData : WindowProperties
{
    public Dictionary<string,UserData> AllUsers = new Dictionary<string,UserData>();
}


public class SpriteSendProperties : ISpriteProperties
{
    private Sprite _sprite;
    public Sprite Sprite
    {
        get => _sprite;
        set => _sprite = value;
    }

    public string Email;
}
public class UsersScreen : AWindowController<UsersScreenData>
{
    public GameObject userPrefab;
    public Transform content;
    public AudioClip viewDetailButtonAudioclip;
    [SerializeField] private string url = "https://randomuser.me/api/?results=50&inc=name,email,gender,phone,dob,picture";
    private List<User> _cells = new List<User>();
    [SerializeField] private Scrollbar _scrollbar;
    public async Task ReadUsersData()
    {
        NetworkManager request = ServiceLocator.Instance.Get<NetworkManager>();
        UsersData data = await request.Get<UsersData>(url);
        foreach(Result user in data.results)
        {
            Properties.AllUsers.Add(user.email ,new UserData(user.name.first, user.name.last, user.email, user.gender, user.phone, user.dob.age, user.picture.large, user.picture.thumbnail ));
        }
    }

    public UsersScreenData GetUsersData()
    {
        return Properties;
    }

    public void ShowUserDetailScreen(string email)
    {
        SpriteSendProperties spriteSendProperties = new SpriteSendProperties();
        spriteSendProperties.Email = email;
        AssetManager assetManager = ServiceLocator.Instance.Get<AssetManager>();
        assetManager.GetSprite(Properties.AllUsers[email].imageUrl, spriteSendProperties ,ProfilePictureCallBack);
    }
    
    private void ProfilePictureCallBack(ISpriteProperties temp)
    {
        SpriteSendProperties data = (SpriteSendProperties)temp;
        Properties.AllUsers[data.Email].image = data.Sprite;
        UIFrame uiFrame = ServiceLocator.Instance.Get<UIFrame>();
        uiFrame.OpenWindow("UserDetail Screen", Properties.AllUsers[data.Email]);
    }
    
    protected override void OnPropertiesSet() {
        foreach (var userPair in Properties.AllUsers)
        {
            UserData user = userPair.Value;
            GameObject userObject = Instantiate(userPrefab, content);
            User info = userObject.GetComponent<User>();
            _cells.Add(info);
            info.Initialize(user, this);
        }
    }
    
    public void OnSettingClick()
    {
        UIFrame uiFrame = ServiceLocator.Instance.Get<UIFrame>();
        uiFrame.OpenWindow("Setting Screen");
    }

    public void OnBackClick()
    {
        CleanUp();
        UIFrame uiFrame = ServiceLocator.Instance.Get<UIFrame>();
        uiFrame.CloseWindow("Users Screen");
    }
    public void CleanUp()
    {
        for(int i = 0; i < _cells.Count; i++)
        {
            Destroy(_cells[i].gameObject);
        }
        _cells.Clear();
        Properties.AllUsers.Clear();
        _scrollbar.value = 1f;
    }
}
