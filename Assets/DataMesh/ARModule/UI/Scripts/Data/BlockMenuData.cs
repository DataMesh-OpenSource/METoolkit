using UnityEngine;
using System.Collections.Generic;


namespace DataMesh.AR.UI
{

    [System.Serializable]
    public class BlockMenuData
    {
        public string name;
        public BlockPanelData rootPanel = null;
        
    }

    [System.Serializable]
    public class BlockPanelData
    {
        //public GameObject[] buttonPrefabs = new GameObject[4];
        public List<BlockButtonData> buttons = new List<BlockButtonData>();


    }

    [System.Serializable]
    public class BlockButtonData
    {
        public string buttonId;
        public string buttonName;
        public BlockButtonColor buttonColor = new BlockButtonColor();
        public string buttonPic;
        public bool canClick = true;

        public BlockPanelData subPanel = null;


    }

    [System.Serializable]
    public class BlockButtonColor
    {
        public float r = 3f / 255f;
        public float g = 169f / 255f;
        public float b = 244f / 255f;
        public float a = 1f;

        public static bool operator ==(BlockButtonColor lhs, BlockButtonColor rhs)
        {
            return (lhs.r == rhs.r) && (lhs.g == rhs.g) && (lhs.b == rhs.b) && (lhs.a == rhs.a);
        }

        public static bool operator !=(BlockButtonColor lhs, BlockButtonColor rhs)
        {
            return (lhs.r != rhs.r) || (lhs.g != rhs.g) || (lhs.b != rhs.b) || (lhs.a != rhs.a);
        }

        public override bool Equals(object other)
        {
            BlockButtonColor c = (BlockButtonColor)other;
            return (this == c);
        }

        public override int GetHashCode()
        {
            return BlockButtonColor.ColorToInt(this);
        }

        public static implicit operator BlockButtonColor(Color c)
        {
            BlockButtonColor rs = new BlockButtonColor();
            rs.r = c.r;
            rs.g = c.g;
            rs.b = c.b;
            rs.a = c.a;
            return rs;
        }
        public static implicit operator Color(BlockButtonColor c)
        {
            Color rs = new Color(c.r, c.g, c.b, c.a);
            return rs;
        }


        static public int ColorToInt(BlockButtonColor c)
        {
            int retVal = 0;
            retVal |= Mathf.RoundToInt(c.r * 255f) << 24;
            retVal |= Mathf.RoundToInt(c.g * 255f) << 16;
            retVal |= Mathf.RoundToInt(c.b * 255f) << 8;
            retVal |= Mathf.RoundToInt(c.a * 255f);
            return retVal;
        }
    }
}