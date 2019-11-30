using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OntologyManager))]

public class SemanticRoomManager : MonoBehaviour
{
    public int _nObservationsToConsider = 5;
    public List<SemanticRoom> _semantic_rooms;
    public GameObject _panel;
    public Image[] _bars;
    public Text[] _leyend;
    public Text _title;


    private OntologyManager _ontologyManager;    

    void Start()
    {
        _ontologyManager = GetComponent<OntologyManager>();
    }

    public void LookForRooms() {
        _semantic_rooms = new List<SemanticRoom>(FindObjectsOfType<SemanticRoom>());
        _semantic_rooms.ForEach(sr => sr.InitializeCategories(_ontologyManager._cateogiesOfRooms));
        _ontologyManager.AddNewRooms(_semantic_rooms);        
    }

    public SemanticRoom FindSemanticRoomOf(Vector3 center) {
        return _semantic_rooms.Find(sr => sr.PointInside(center));
    }

    public void UpdateRoom(SemanticRoom room) {

        if (room != null)
        {
            var categories = _ontologyManager.GetCategoryProbability(room._id, _nObservationsToConsider);

            room._categories = categories;

            String[] strlist = room._id.Split('_');

            _title.text = strlist[1];
            _leyend[0].text = (categories[_ontologyManager.GetNameWithURI("Bathroom")]).ToString(".00");
            _bars[0].fillAmount = (categories[_ontologyManager.GetNameWithURI("Bathroom")]);
            _leyend[1].text = (categories[_ontologyManager.GetNameWithURI("Bedroom")]).ToString(".00");
            _bars[1].fillAmount = (categories[_ontologyManager.GetNameWithURI("Bedroom")]);
            _leyend[2].text = (categories[_ontologyManager.GetNameWithURI("Dressing_Room")]).ToString(".00");
            _bars[2].fillAmount = (categories[_ontologyManager.GetNameWithURI("Dressing_Room")]);
            _leyend[3].text = (categories[_ontologyManager.GetNameWithURI("Kitchen")]).ToString(".00");
            _bars[3].fillAmount = (categories[_ontologyManager.GetNameWithURI("Kitchen")]);
            _leyend[4].text = (categories[_ontologyManager.GetNameWithURI("Living_Room")]).ToString(".00");
            _bars[4].fillAmount = (categories[_ontologyManager.GetNameWithURI("Living_Room")]);

            _panel.SetActive(true);
        }
    }

    public void PrintResult() {
        foreach (SemanticRoom sr in _semantic_rooms) {
            PrintResult(sr);
        }
    }

    public void PrintResult(SemanticRoom semanticRoom) {

        var categories = semanticRoom._categories;

        //Print Result
        string tx = semanticRoom._id + ":\r\n";

        foreach (KeyValuePair<string, float> c in categories)
        {
            tx += c.Key + ": " + c.Value + " \r\n";
        }
        Debug.Log(tx);
    }

}
