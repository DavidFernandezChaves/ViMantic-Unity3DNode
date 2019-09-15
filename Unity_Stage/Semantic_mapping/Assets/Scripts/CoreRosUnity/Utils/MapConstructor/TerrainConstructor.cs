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

        ////Alturas
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

        //Texturas
        //SplatPrototype[] terrainTexture = new SplatPrototype[1];
        //terrainTexture[0] = new SplatPrototype
        //{
        //    texture = texture,
        //    tileSize = new Vector2(2, 2)
        //};
        //td.splatPrototypes = terrainTexture;


        td.terrainLayers[0] = new TerrainLayer() { normalMapTexture = texture };

        t = Terrain.CreateTerrainGameObject(td).GetComponent<Terrain>();
        t.transform.position = _originPosition;
        t.gameObject.layer = 18;

        gameObject.SendMessage("MapLoaded",t, SendMessageOptions.DontRequireReceiver);

        //// This creates a Grid Graph
        //GridGraph gg = (GridGraph) AstarPath.active.graphs[0];
        //// Updates internal size from the above values
        //float w = (_width *_resolutionMap)/_resolution;
        //float h = (_height * _resolutionMap) / _resolution;
        //gg.SetDimensions((int)w,(int)h , _resolution);      
        //gg.center = new Vector3(t.transform.position.x+(_width * _resolutionMap)/2, 0, t.transform.position.z+ (_height * _resolutionMap)/2);
        //gg.erodeIterations = (int) Mathf.Ceil((_erode/2)/_resolution) ; 

        //// Scans all graphs
        //AstarPath.active.Scan();

        //var texbox = FindObjectOfType<HeadManaguer>();
        //if (texbox != null) {
        //    texbox.Said("Estoy listo para la accion");
        //}
       
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
