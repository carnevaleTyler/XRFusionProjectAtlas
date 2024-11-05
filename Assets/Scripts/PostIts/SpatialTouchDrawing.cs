using System.Collections.Generic;
using Fusion.Addons.TextureDrawing;
using Fusion.Addons.VisionOsHelpers;
using Fusion.XR.Shared.Rig;
using Fusion.XR.Shared.Touch;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;


/***
 * 
 * The SpatialTouchDrawing class extends the TouchDrawing class to support spatial touch input on VisionOS. 
 * Two drawing mode are supported : Spatial touch or finger touch.
 * The class override FindDrawer() to find the appropriate TextureDrawer for spatial touch drawing. 
 * It implements the ISpatialTouchListener interface to handles the drawing functionality when a spatial touch interaction begins or ends. 
 * 
 ***/
public class SpatialTouchDrawing : TouchDrawing, ISpatialTouchListener
{
    [System.Flags]
    public enum VisionOSDrawingMode
    {
        None = 0,
        SpatialTouch = 1,
        IndexToucher = 2,
        Both = VisionOSDrawingMode.SpatialTouch | VisionOSDrawingMode.IndexToucher
    }

    [Header("VisionOS")]
    [SerializeField] VisionOSDrawingMode visionOSDrawingMode = VisionOSDrawingMode.SpatialTouch;

    List<Vector3> positionsForSpatialTouch = new List<Vector3>();
    TextureDrawer drawerForSpatialTouch;

    protected bool IsSpatialTouchDrawingModeEnabled => Application.platform == RuntimePlatform.VisionOS && ((visionOSDrawingMode & VisionOSDrawingMode.SpatialTouch) != 0);
    protected override bool IsIndexToucherDrawingModeEnabled => Application.platform != RuntimePlatform.VisionOS || ((visionOSDrawingMode & VisionOSDrawingMode.IndexToucher) != 0);

    protected override TextureDrawer FindDrawer(Toucher toucher)
    {
        // First, check drawer for regular the hands, before looking for the spatial touch one (one without associated toucher)
        if (toucher != null)
        {
            return base.FindDrawer(toucher);
        }
        else
        {
            // Look for cached version
            if (drawerForSpatialTouch)
            {
                return drawerForSpatialTouch;
            }

            TextureDrawer drawer = null;
            foreach (var d in FindObjectsOfType<TextureDrawer>())
            {
                if (d.Object.HasStateAuthority)
                {
                    NetworkHand networkHand = d.GetComponentInParent<NetworkHand>();
                    if (networkHand == null)
                    {
                        drawer = d;
                        break;
                    }
                }
            }
            drawerForSpatialTouch = drawer;
            return drawer;
        }
    }

    protected override List<Vector3> PositionCache(Toucher toucher = null)
    {
        if (toucher)
        {
            return base.PositionCache(toucher);
        }
        return positionsForSpatialTouch;
    }

    #region ISpatialTouchListener
    public void TouchStart(SpatialPointerKind interactionKind, Vector3 interactionPosition, SpatialPointerState primaryTouchData)
    {
        if (IsSpatialTouchDrawingModeEnabled == false) return;
        if (interactionKind != SpatialPointerKind.Touch) return;

        currentFingerStatus = FingerStatus.Drawing;
        if (currentFingerStatus == FingerStatus.Drawing)
        {
            Draw(interactionPosition);
            AudioFeedback();
        }
    }

    public void TouchStay(SpatialPointerKind interactionKind, Vector3 interactionPosition, SpatialPointerState primaryTouchData)
    {
        if (IsSpatialTouchDrawingModeEnabled == false) return;
        if (currentFingerStatus == FingerStatus.Drawing)
        {
            if (interactionKind != SpatialPointerKind.Touch) return;
            Draw(interactionPosition);
            AudioFeedback();
        }

    }

    public void TouchEnd()
    {
        if (IsSpatialTouchDrawingModeEnabled == false) return;
        if (currentFingerStatus == FingerStatus.Drawing)
        {
            positionsForSpatialTouch.Clear();
            EndLine();
            AudioFeedback();
        }
    }
    #endregion

}
