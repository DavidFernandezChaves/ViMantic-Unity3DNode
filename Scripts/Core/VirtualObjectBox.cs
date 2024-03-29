﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViMantic
{
    public class VirtualObjectBox : MonoBehaviour
    {
        [Header("General")]
        [Tooltip("The log level to use")]
        public LogLevel LogLevel = LogLevel.Normal;

        public Vector2 heightCanvas;

        [SerializeReference]
        public SemanticObject semanticObject;
        public Color BoxColor { get; private set; }

        public CanvasLabelClass canvasLabel;
        public LineRenderer lineRender;

        #region Unity Functions

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying && this.enabled && LogLevel == LogLevel.Developer)
            {
                for (int i = 0; i < 8; i++)
                {

                    if (semanticObject.Corners[i].occluded)
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.green;

                    //if (semanticObject.Defined) Gizmos.color = Color.blue;

                    //if (i == 0) Gizmos.color = Color.blue;
                    //if (i == 1) Gizmos.color = Color.magenta;
                    //if (i == 2) Gizmos.color = Color.gray;
                    //if (i == 3) Gizmos.color = Color.yellow;
                    //if (i == 4) Gizmos.color = Color.green;
                    //if (i == 5) Gizmos.color = Color.red;
                    //if (i == 6) Gizmos.color = Color.white;
                    //if (i == 7) Gizmos.color = Color.cyan;

                    Gizmos.DrawSphere(semanticObject.Corners[i].position, 0.02f);
                }
            }
        }
#endif


        private void Start()
        {
            BoxColor = VirtualObjectSystem.instance.GetColorObject(this);
            Material material = new Material(Shader.Find("Standard"));
            GetComponent<Renderer>().materials[0] = material;
            GetComponent<Renderer>().material.SetFloat("_Mode", 2.0f);            
            GetComponent<Renderer>().material.SetColor("_UnlitColor", BoxColor);
            if (LogLevel == LogLevel.Developer && !semanticObject.Defined)
            {
                GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 0, 0, 0.1f));
            }
            else
            {
                GetComponent<Renderer>().material.SetColor("_Color", new Color(BoxColor.r, BoxColor.g, BoxColor.b, 0.3f));
            }

            lineRender.startColor = BoxColor;
            lineRender.endColor = BoxColor;
            canvasLabel.SetColor(BoxColor);
            UpdateObject();
            var room = GetRoom(transform.position);
            if (room != null)
                semanticObject.SetRoom(room);
            else
                RemoveVirtualBox();

        }
        #endregion

        #region Public Functions
        public void InitializeSemanticObject(SemanticObject _semanticObject)
        {
            semanticObject = _semanticObject;
        }

        public void UpdateObject()
        {
            if (semanticObject.ObjectClass == "Other")
            {
                RemoveVirtualBox();
                return;
            }

            transform.parent.name = semanticObject.NDetections + "_" + semanticObject.ObjectClass + "_" + semanticObject.Id;

            //Load Object
            transform.parent.position = semanticObject.Position;
            transform.localScale = semanticObject.Size;
            transform.parent.rotation = semanticObject.Rotation;

            GetComponent<MeshRenderer>().enabled = true;

            canvasLabel.gameObject.SetActive(true);
            lineRender.enabled = true;

            //Load Canvas
            canvasLabel.transform.position = semanticObject.Position + new Vector3(0, UnityEngine.Random.Range(heightCanvas.x, heightCanvas.y), 0);
            canvasLabel.LoadLabel(semanticObject.ObjectClass, semanticObject.Score);

            lineRender.SetPosition(0, canvasLabel.transform.position - new Vector3(0, 0.2f, 0));
            lineRender.SetPosition(1, transform.parent.position);


            if (LogLevel == LogLevel.Developer && !semanticObject.Defined)
            {
                GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 0, 0, 0.1f));
            }
            else
            {
                GetComponent<Renderer>().material.SetColor("_Color", new Color(BoxColor.r, BoxColor.g, BoxColor.b, 0.3f));
            }
        }

        public void RemoveVirtualBox()
        {
            //OntologySystem.instance.RemoveSemanticObject(semanticObject);
            VirtualObjectSystem.instance.UnregisterColor(BoxColor);
            Destroy(transform.parent.gameObject);
        }

        public void NewDetection(SemanticObject newDetection)
        {
            semanticObject.NewDetection(newDetection);
            UpdateObject();
        }

        public static SemanticRoom GetRoom(Vector3 position)
        {
            RaycastHit hit;
            position.y = -100;
            if (Physics.Raycast(position, Vector3.up, out hit))
            {
                return hit.transform.GetComponent<SemanticRoom>();
            }
            return null;
        }
        #endregion

        #region Private Functions
        private void Log(string _msg, LogLevel lvl, bool Warning = false)
        {
            if (LogLevel <= lvl && LogLevel != LogLevel.Nothing)
            {
                if (Warning)
                {
                    Debug.LogWarning("[Virtual Object Box]: " + _msg);
                }
                else
                {
                    Debug.Log("[Virtual Object Box]: " + _msg);
                }
            }
        }
        #endregion

    }
}