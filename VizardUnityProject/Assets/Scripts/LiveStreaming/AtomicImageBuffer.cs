/*
 ISC License

 Copyright (c) 2025, Autonomous Vehicle Systems Lab, University of Colorado at Boulder

 Permission to use, copy, modify, and/or distribute this software for any
 purpose with or without fee is hereby granted, provided that the above
 copyright notice and this permission notice appear in all copies.

 THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

 */

using System.Threading;

/// <summary>
/// Atomic buffer that stores and sends to Basilisk the most recently requested camera image 
/// </summary>
public static class AtomicImageBuffer
{
    private static SemaphoreSlim request_pending = new SemaphoreSlim(0, 1);
    private static Mutex atomic_access = new Mutex();

    private static bool isRequestPending; //True if Basilisk is awaiting a requested image

    public static byte[] ImageBuffer; //Stores the camera image to be sent to Basilisk

    public static int CameraID = -1; //Camera ID of instrument camera whose image is being requested

    public static bool IsRequestPending => isRequestPending;

    /// <summary>
    /// Set up request for a new image to be taken by specified instrument camera
    /// </summary>
    /// <param name="cameraID">Camera ID of instrument camera to take image</param>
    public static void RequestScreenshot(int cameraID)
    {
        atomic_access.WaitOne();
        CameraID = cameraID;
        isRequestPending = true;
        atomic_access.ReleaseMutex();
        request_pending.Wait();
    }

    /// <summary>
    /// Lock the image buffer 
    /// </summary>
    public static void LockBuffer()
    {
        atomic_access.WaitOne();
    }

    /// <summary>
    /// Release the image buffer and resume playback of messages
    /// </summary>
    public static void ReleaseBuffer()
    {
        atomic_access.ReleaseMutex();
        MessageList.PlaybackPaused = false;
    }

    /// <summary>
    /// Signal Basilisk that the camera image has been provided and the image request is fulfilled
    /// </summary>
    public static void SignalScreenshotFulfilled()
    {
        atomic_access.WaitOne();
        atomic_access.ReleaseMutex();
        isRequestPending = false;
        request_pending.Release();
    }
}