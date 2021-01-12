using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotAtVirtualHome {

    [RequireComponent(typeof(ObjectManager))]
    [RequireComponent(typeof(OntologyManager))]

    public class GeneralManager : MonoBehaviour {

        public int verbose;

        public string _nameMap;
        public bool saveOntology;
        private OntologyManager ontologyManager;


        #region Unity Functions
        private void Awake() {
            ontologyManager = GetComponent<OntologyManager>();

        }

        void Start() {
            _nameMap = PlayerPrefs.GetString("nameMap", "Semantic map 1");
            ontologyManager.LoadOntology(_nameMap);
        }

        private void OnDestroy() {
            if (saveOntology) {
                ontologyManager.SaveOntology();
                PlayerPrefs.SetString("nameMap", _nameMap);
            }
        }
        #endregion

        #region Public Functions
        public void OnVirtualEnviromentLoaded() {

        }
        #endregion

        #region Private Functions
        private void Log(string _msg) {
            if (verbose > 1)
                Debug.Log("[General Manager]: " + _msg);
        }

        private void LogWarning(string _msg) {
            if (verbose > 0)
                Debug.LogWarning("[General Manager]: " + _msg);
        }
        #endregion

    }
}

