using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minesweeper.Debug
{
    public class NoiseTest : MonoBehaviour
    {
        public UnityEngine.UI.RawImage image;
        public int width, height;
        public float xOrigin, yOrigin;
        public float scale;
        [Range(0, 7)]
        public int octaves = 0;
        [Range(0.0f, 1.0f)]
        public float threshold = 0f;
        [Range(0.01f, 0.2f)]
        public float range = 0.05f;
        public CutoffMode cutoff;

        public bool allowChangesInUpdate;
        private float lastScale, lastXOrigin, lastYOrigin, lastThreshold, lastRange;
        private int lastOctaves;
        private CutoffMode lastCutoff;

        Texture2D tex;

        // Start is called before the first frame update
        void Awake()
        {
            //generate random texture.
            tex = new Texture2D(width, height);
            GenerateNoise();
        }

        private void Update()
        {
            if(allowChangesInUpdate)
            {
                if(lastScale != scale || lastXOrigin != xOrigin || lastYOrigin != yOrigin 
                    || lastOctaves != octaves || lastThreshold != threshold 
                    || lastCutoff != cutoff || lastRange != range)
                {
                    lastXOrigin = xOrigin;
                    lastYOrigin = yOrigin;
                    lastOctaves = octaves;
                    lastScale = scale;
                    lastThreshold = threshold;
                    lastRange = range;
                    lastCutoff = cutoff;
                    GenerateNoise();
                }
            }
        }

        Color[] colors;
        void GenerateNoise()
        {
            colors = new Color[width * height];
            float maxAmplitude = 1f;
            for (float x = 0; x < width; x++)
            {
                for (float y = 0; y < height; y++)
                {
                    float xCoord = xOrigin + x / width;
                    float yCoord = yOrigin + y / height;
                    float noiseResult = Mathf.PerlinNoise(xCoord * scale, yCoord * scale);

                    for(float oct = 0; oct < octaves; oct++)
                    {
                        float scaleOffset = Mathf.Pow(oct, 1.5f) * 5f;
                        noiseResult += Mathf.PerlinNoise(xCoord * (scale + scaleOffset), yCoord * (scale + scaleOffset)) / (oct+2f);
                    }
                    //is the noise result higher than the max amplitude?
                    if (noiseResult > maxAmplitude)
                        maxAmplitude = noiseResult;
                    
                    colors[(int)y * width + (int)x] = new Color(noiseResult, noiseResult, noiseResult);
                }
            }
            if (octaves != 0 || cutoff != CutoffMode.None)
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    var col = colors[i];
                    col /= maxAmplitude; //normalize colours

                    if (cutoff == CutoffMode.Flat) //flat cutoff
                        col.r = (col.r > threshold) ? col.r : 0f;
                    else if (cutoff == CutoffMode.FlatBiDirectional)
                        col.r = (col.r > threshold) ? 1f : 0f;
                    else if (cutoff == CutoffMode.Invert)
                        col.r = (col.r > threshold) ? col.r : 1f - col.r;
                    else if (cutoff == CutoffMode.ProcMap)
                    {
                        col.r = (col.r > threshold) ? col.r : 1f - col.r;
                        col.r = InRange(col.r, threshold, range) ? 0f : 1f;
                    }
                    else if (cutoff == CutoffMode.Exponential) //exponential cutoff
                        col.r = (col.r > threshold) ? col.r : Mathf.Pow(col.r, 2);
                    col.b = col.r;
                    col.g = col.r;
                    col.a = 1;
                    colors[i] = col;
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            image.texture = tex; 
        }

        bool InRange(float value, float center, float radius)
        {
            var v = center - value;
            return radius >= v && -radius <= v;
        }

        [System.Serializable]
        public enum CutoffMode
        {
            Flat, Exponential, None, FlatBiDirectional, Invert, ProcMap
        }

    }
}