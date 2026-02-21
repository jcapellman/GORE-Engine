using System;
using System.Runtime.InteropServices;

namespace GORE.Engine.Renderers
{
    // Minimal P/Invoke for EGL/ANGLE interop
    public static class EglNative
    {
        public const int EGL_DEFAULT_DISPLAY = 0;
        public const int EGL_NO_CONTEXT = 0;
        public const int EGL_NO_SURFACE = 0;
        public const int EGL_NO_DISPLAY = 0;

        [DllImport("libEGL.dll")]
        public static extern IntPtr eglGetDisplay(IntPtr display_id);

        [DllImport("libEGL.dll")]
        public static extern int eglInitialize(IntPtr dpy, out int major, out int minor);

        [DllImport("libEGL.dll")]
        public static extern IntPtr eglCreateWindowSurface(IntPtr dpy, IntPtr config, IntPtr win, int[] attrib_list);

        [DllImport("libEGL.dll")]
        public static extern IntPtr eglCreateContext(IntPtr dpy, IntPtr config, IntPtr share_context, int[] attrib_list);

        [DllImport("libEGL.dll")]
        public static extern int eglMakeCurrent(IntPtr dpy, IntPtr draw, IntPtr read, IntPtr ctx);

        [DllImport("libEGL.dll")]
        public static extern int eglSwapBuffers(IntPtr dpy, IntPtr surface);

        [DllImport("libEGL.dll")]
        public static extern int eglDestroyContext(IntPtr dpy, IntPtr ctx);

        [DllImport("libEGL.dll")]
        public static extern int eglDestroySurface(IntPtr dpy, IntPtr surface);

        [DllImport("libEGL.dll")]
        public static extern int eglTerminate(IntPtr dpy);
    }
}
