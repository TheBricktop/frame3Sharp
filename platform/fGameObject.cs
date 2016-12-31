﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using g3;

namespace f3
{

    //
    // fGameObject wraps a GameObject for frame3Sharp. The idea is that eventually we
    //  will be able to "replace" GameObject with something else, ie non-Unity stuff.
    //
    // implicit cast operators allow transparent conversion between GameObject and fGameObject
    //
    public class fGameObject
    {
        protected GameObject go;

        public fGameObject(GameObject go)
        {
            this.go = go;
            PreRenderBehavior pb = go.AddComponent<PreRenderBehavior>();
            pb.ParentFGO = this;
        }



        // allow game object wrapper to do things here, eg lazy updates, etc
        // This will be called by the GameObject itself, using the PreRenderBehavior (!)
        public virtual void PreRender()
        {
        }


        public virtual void SetName(string name)
        {
            go.name = name;
        }
        public virtual string GetName()
        {
            return go.name;
        }

        public virtual void SetLayer(int layer)
        {
            go.layer = layer;
        }
        public virtual int GetLayer()
        {
            return go.layer;
        }

        public virtual bool HasChildren()
        {
            return go.transform.childCount > 0;
        }
        public virtual System.Collections.IEnumerable Children()
        {
            for (int k = 0; k < go.transform.childCount; ++k)
                yield return go.transform.GetChild(k).gameObject;
        }


        public virtual Mesh GetMesh()
        {
            return go.GetComponent<MeshFilter>().mesh;
        }
        public virtual Mesh GetSharedMesh()
        {
            return go.GetComponent<MeshFilter>().sharedMesh;
        }
        public virtual void SetMesh(Mesh m)
        {
            go.GetComponent<MeshFilter>().mesh = m;
        }
        public virtual void SetSharedMesh(Mesh m)
        {
            go.GetComponent<MeshFilter>().sharedMesh = m;
        }

        public virtual void SetMaterial(fMaterial mat)
        {
            go.GetComponent<Renderer>().material = mat;
        }
        public virtual fMaterial GetMaterial()
        {
            return new fMaterial(go.GetComponent<Renderer>().material);
        }

        public virtual void SetColor(Colorf color)
        {
            Renderer r = go.GetComponent<Renderer>();
            r.material.color = color;
        }


        public void SetParent(fGameObject parentGO, bool bKeepWorldPosition = false)
        {
            if (parentGO == null)
                go.transform.parent = null;
            else
                go.transform.SetParent(((GameObject)parentGO).transform, bKeepWorldPosition);
        }



        public virtual void SetPosition(Vector3f vPosition)
        {
            go.transform.position = vPosition;
        }
        public virtual Vector3f GetPosition()
        {
            return go.transform.position;
        }

        public virtual void SetLocalPosition(Vector3f vPosition)
        {
            go.transform.localPosition = vPosition;
        }
        public virtual Vector3f GetLocalPosition()
        {
            return go.transform.localPosition;
        }

        public virtual void SetLocalScale(Vector3f vScale)
        {
            go.transform.localScale = vScale;
        }
        public virtual void SetLocalScale(float fScale)
        {
            go.transform.localScale = fScale * Vector3f.One; 
        }
        public virtual Vector3f GetLocalScale()
        {
            return go.transform.localScale;
        }


        public virtual Frame3f GetWorldFrame() {
            return UnityUtil.GetGameObjectFrame(go, CoordSpace.WorldCoords);
        }
        public virtual void SetWorldFrame(Frame3f f) {
            UnityUtil.SetGameObjectFrame(go, f, CoordSpace.WorldCoords);
        }

        public virtual Frame3f GetLocalFrame() {
            return UnityUtil.GetGameObjectFrame(go, CoordSpace.ObjectCoords);
        }
        public virtual void SetLocalFrame(Frame3f f) {
            UnityUtil.SetGameObjectFrame(go, f, CoordSpace.ObjectCoords);
        }


        public static implicit operator UnityEngine.GameObject(fGameObject go)
        {
            return go.go;
        }
        public static implicit operator fGameObject(UnityEngine.GameObject go)
        {
            return new fGameObject(go);
        }
    }






    public class fTextGameObject : fGameObject
    {
        public fTextGameObject(GameObject go) : base(go)
        {
        }

        public void SetText(string sText)
        {
            TextMesh tm = go.GetComponent<TextMesh>();
            tm.text = sText;
        }

        public override void SetColor(Colorf color)
        {
            TextMesh tm = go.GetComponent<TextMesh>();
            tm.color = color;
        }
    }







    public class fCurveGameObject : fGameObject
    {
        float width = 0.05f;
        Colorf color = Colorf.Black;

        public fCurveGameObject(GameObject go) : base(go)
        {
        }


        public void SetLineWidth(float fWidth) {
            update(fWidth, color);
        }
        public float GetLineWidth() { return width; }

        public override void SetColor(Colorf newColor) {
            update(width, newColor);
        }
        public Colorf GetColor() { return color; }


        protected void update(float newWidth, Colorf newColor)
        {
            LineRenderer r = go.GetComponent<LineRenderer>();
            if (width != newWidth) {
                width = newWidth;
                r.startWidth = r.endWidth = width;
            }
            if (color != newColor) {
                color = newColor;
                r.startColor = r.endColor = color;
                base.SetColor(color);       // material overrides line renderer??
            }
        }

    }





    public class fLineGameObject : fCurveGameObject
    {
        Vector3f start, end;

        public fLineGameObject(GameObject go) : base(go)
        {
            LineRenderer r = go.GetComponent<LineRenderer>();
            r.numPositions = 2;
        }


        public void SetStart(Vector3f s) {
            if ( start != s ) {
                start = s;
                LineRenderer r = go.GetComponent<LineRenderer>();
                r.SetPosition(0, start);
            }
        }
        public Vector3f GetStart() { return start; }


        public void SetEnd(Vector3f e) {
            if ( end != e ) {
                end = e;
                LineRenderer r = go.GetComponent<LineRenderer>();
                r.SetPosition(1, end);
            }
        }
        public Vector3f GetEnd() { return end; }
    }




    public class fCircleGameObject : fCurveGameObject
    {
        float radius = 1.0f;
        int steps = 32;
        bool bCircleValid = false;

        public fCircleGameObject(GameObject go) : base(go)
        {
        }


        public void SetRadius(float fRadius) {
            if ( radius != fRadius ) {
                radius = fRadius;
                bCircleValid = false;
            }
        }
        public float GetRadius() { return radius; }


        public void SetSteps(int nSteps) {
            if ( steps != nSteps) {
                steps = nSteps;
                bCircleValid = false;
            }
        }
        public int GetSteps() { return steps; }

        public override void PreRender()
        {
            if (bCircleValid)
                return;

            LineRenderer r = go.GetComponent<LineRenderer>();
            if (r.numPositions != steps + 1)
                r.numPositions = steps + 1;
            float twopi = (float)(2 * Math.PI);
            for (int i = 0; i <= steps; ++i) {
                float t = (float)i / (float)steps;
                float a = t * twopi;
                float x = radius * (float)Math.Cos(a);
                float y = radius * (float)Math.Sin(a);
                r.SetPosition(i, new Vector3f(x, 0, y));
            }

            bCircleValid = true;
        }
    }






    public class PreRenderBehavior : MonoBehaviour
    {
        public fGameObject ParentFGO = null;
        void Update() {
            ParentFGO.PreRender();
        }
    }



}