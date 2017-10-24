using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Text;


public class GameManager : MonoBehaviour
{

    public static GameManager instance = null;
    public DataStore currentData;
    string path;


    void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        path = Application.dataPath + "/StreamingAssets/";
        //currentData = LoadDataBinary();
        currentData = LoadDataXML();
    }

    //------------------------------------------

    DataStore LoadDataBinary()
    {
        if(File.Exists(path + "StoredData.dat"))
        {
            using (FileStream stream = new FileStream(path + "StoredData.dat", FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(stream) as DataStore;
            }
        }
        return new DataStore();
    }

    //------------------------------------------

    DataStore LoadDataXML()
    {
        if (File.Exists(path + "StoredData.xml"))
        {
            using (StreamReader stream = new StreamReader(path + "StoredData.xml"))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataStore));
                return xmlSerializer.Deserialize(stream) as DataStore;
            }
        }
        return new DataStore();
    }

    //------------------------------------------

    public void SaveDataBinary()
    {
        using (FileStream stream = new FileStream(path + "StoredData.dat", FileMode.Create))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, currentData);
        }
    }

    //----------------------------------------------------------------------

    public void SaveDataXML()
    {
        Encoding encoding = Encoding.GetEncoding("UTF-8");
        using (StreamWriter stream = new StreamWriter(path + "StoredData.xml", false, encoding))
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataStore));
            xmlSerializer.Serialize(stream, currentData);
        }
    }

    //----------------------------------------------------------------------

    public void AStarSlow()
    {
        SceneManager.LoadScene(0);
    }
    public void RecursiveSlow()
    {
        SceneManager.LoadScene(1);
    }
    public void AStarTerrain()
    {
        SceneManager.LoadScene(2);
    }
    public void RecursiveTerrain()
    {
        SceneManager.LoadScene(3);
    }

   
}
