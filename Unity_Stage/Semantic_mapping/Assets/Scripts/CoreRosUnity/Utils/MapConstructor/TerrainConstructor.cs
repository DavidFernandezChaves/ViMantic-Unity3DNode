using ROSBridgeLib.nav_msgs;
using UnityEngine;
using ROSBridgeLib.geometry_msgs;
//using Pathfinding;

public class TerrainConstructor : MonoBehaviour {
    public float _erode = 0.5f;
    public float _resolution=0.2f;
    public float _elevation = 10f;
    public Texture2D texture;

    private Vector3 _originPosition;
    private TerrainData td;
    private Terrain t;
    private float[,] heights;
    private int xRes;
    private int yRes;


    public void NewOcupanceGridMsg(OccupancyGridMsg msg) {
        MapMetaDataMsg metaData = msg.GetInfo();
        sbyte[] AlturasRecibidas = msg.GetData();


        float _resolutionMap = msg.GetInfo().GetResolution();  
        int _width = (int) (metaData.Getwidth());
        int _height = (int) (metaData.Getheight());

        _originPosition = msg.GetInfo().GetOrigin().GetTranslationUnity();

        td = new TerrainData();
        if (_width > _height)
        {
            td.heightmapResolution = _width;
        }
        else
        {
            td.heightmapResolution = _height;
        }

        td.size = new Vector3(_width * _resolutionMap, _elevation, _height * _resolutionMap);

        xRes = td.heightmapWidth;
        yRes = td.heightmapHeight;
        heights = td.GetHeights(0, 0, xRes, yRes);

        for (int filas = 0; filas < yRes; filas++)
        {
            for (int columna = 0; columna < xRes; columna++)
            {
                heights[filas,columna] = AlturasRecibidas[(int)(columna*_width/xRes) + (int)(filas*_height/yRes) * _width] / 100;
            }
        }

        td.SetHeights(0, 0, heights);

        td.terrainLayers = new TerrainLayer[1] { new TerrainLayer { diffuseTexture = texture, tileSize = new Vector2(2, 2) } };

        t = Terrain.CreateTerrainGameObject(td).GetComponent<Terrain>();
        t.transform.position = _originPosition;
        t.gameObject.layer = 18;

        gameObject.SendMessage("MapLoaded",t, SendMessageOptions.DontRequireReceiver);
       
    }

    public void ChangeResolution(string s) {
        try
        {
            _resolution = float.Parse(s);
        }
        catch { Debug.Log("Resolution not valid"); }
    }
    public void ChangeRobotSize(string s) {
        try
        {
            _erode = float.Parse(s);
        }
        catch { Debug.Log("Robot Size not valid"); }
    }
}
