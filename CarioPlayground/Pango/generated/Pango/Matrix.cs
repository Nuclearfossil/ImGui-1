// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace Pango {

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

#region Autogenerated code
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Matrix : IEquatable<Matrix> {

		public double Xx;
		public double Xy;
		public double Yx;
		public double Yy;
		public double X0;
		public double Y0;

		public static Pango.Matrix Zero = new Pango.Matrix ();

		public static Pango.Matrix New(IntPtr raw) {
			if (raw == IntPtr.Zero)
				return Pango.Matrix.Zero;
			return (Pango.Matrix) Marshal.PtrToStructure (raw, typeof (Pango.Matrix));
		}

		[DllImport("libpango-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void pango_matrix_concat(IntPtr raw, IntPtr new_matrix);

		public void Concat(Pango.Matrix new_matrix) {
			IntPtr this_as_native = System.Runtime.InteropServices.Marshal.AllocHGlobal (System.Runtime.InteropServices.Marshal.SizeOf (this));
			System.Runtime.InteropServices.Marshal.StructureToPtr (this, this_as_native, false);
			IntPtr native_new_matrix = GLib.Marshaller.StructureToPtrAlloc (new_matrix);
			pango_matrix_concat(this_as_native, native_new_matrix);
			ReadNative (this_as_native, ref this);
			System.Runtime.InteropServices.Marshal.FreeHGlobal (this_as_native);
			new_matrix = Pango.Matrix.New (native_new_matrix);
			Marshal.FreeHGlobal (native_new_matrix);
		}

		[DllImport("libpango-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern double pango_matrix_get_font_scale_factor(IntPtr raw);

		public double FontScaleFactor { 
			get {
				IntPtr this_as_native = System.Runtime.InteropServices.Marshal.AllocHGlobal (System.Runtime.InteropServices.Marshal.SizeOf (this));
				System.Runtime.InteropServices.Marshal.StructureToPtr (this, this_as_native, false);
				double raw_ret = pango_matrix_get_font_scale_factor(this_as_native);
				double ret = raw_ret;
				ReadNative (this_as_native, ref this);
				System.Runtime.InteropServices.Marshal.FreeHGlobal (this_as_native);
				return ret;
			}
		}

		[DllImport("libpango-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr pango_matrix_get_type();

		public static GLib.GType GType { 
			get {
				IntPtr raw_ret = pango_matrix_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[DllImport("libpango-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void pango_matrix_rotate(IntPtr raw, double degrees);

		public void Rotate(double degrees) {
			IntPtr this_as_native = System.Runtime.InteropServices.Marshal.AllocHGlobal (System.Runtime.InteropServices.Marshal.SizeOf (this));
			System.Runtime.InteropServices.Marshal.StructureToPtr (this, this_as_native, false);
			pango_matrix_rotate(this_as_native, degrees);
			ReadNative (this_as_native, ref this);
			System.Runtime.InteropServices.Marshal.FreeHGlobal (this_as_native);
		}

		[DllImport("libpango-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void pango_matrix_scale(IntPtr raw, double scale_x, double scale_y);

		public void Scale(double scale_x, double scale_y) {
			IntPtr this_as_native = System.Runtime.InteropServices.Marshal.AllocHGlobal (System.Runtime.InteropServices.Marshal.SizeOf (this));
			System.Runtime.InteropServices.Marshal.StructureToPtr (this, this_as_native, false);
			pango_matrix_scale(this_as_native, scale_x, scale_y);
			ReadNative (this_as_native, ref this);
			System.Runtime.InteropServices.Marshal.FreeHGlobal (this_as_native);
		}

		[DllImport("libpango-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void pango_matrix_transform_distance(IntPtr raw, ref double dx, ref double dy);

		public void TransformDistance(ref double dx, ref double dy) {
			IntPtr this_as_native = System.Runtime.InteropServices.Marshal.AllocHGlobal (System.Runtime.InteropServices.Marshal.SizeOf (this));
			System.Runtime.InteropServices.Marshal.StructureToPtr (this, this_as_native, false);
			pango_matrix_transform_distance(this_as_native, ref dx, ref dy);
			ReadNative (this_as_native, ref this);
			System.Runtime.InteropServices.Marshal.FreeHGlobal (this_as_native);
		}

		[DllImport("libpango-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void pango_matrix_transform_pixel_rectangle(IntPtr raw, IntPtr rect);

		public void TransformPixelRectangle(ref Pango.Rectangle rect) {
			IntPtr this_as_native = System.Runtime.InteropServices.Marshal.AllocHGlobal (System.Runtime.InteropServices.Marshal.SizeOf (this));
			System.Runtime.InteropServices.Marshal.StructureToPtr (this, this_as_native, false);
			IntPtr native_rect = GLib.Marshaller.StructureToPtrAlloc (rect);
			pango_matrix_transform_pixel_rectangle(this_as_native, native_rect);
			ReadNative (this_as_native, ref this);
			System.Runtime.InteropServices.Marshal.FreeHGlobal (this_as_native);
			rect = Pango.Rectangle.New (native_rect);
			Marshal.FreeHGlobal (native_rect);
		}

		[DllImport("libpango-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void pango_matrix_transform_point(IntPtr raw, ref double x, ref double y);

		public void TransformPoint(ref double x, ref double y) {
			IntPtr this_as_native = System.Runtime.InteropServices.Marshal.AllocHGlobal (System.Runtime.InteropServices.Marshal.SizeOf (this));
			System.Runtime.InteropServices.Marshal.StructureToPtr (this, this_as_native, false);
			pango_matrix_transform_point(this_as_native, ref x, ref y);
			ReadNative (this_as_native, ref this);
			System.Runtime.InteropServices.Marshal.FreeHGlobal (this_as_native);
		}

		[DllImport("libpango-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void pango_matrix_transform_rectangle(IntPtr raw, IntPtr rect);

		public void TransformRectangle(ref Pango.Rectangle rect) {
			IntPtr this_as_native = System.Runtime.InteropServices.Marshal.AllocHGlobal (System.Runtime.InteropServices.Marshal.SizeOf (this));
			System.Runtime.InteropServices.Marshal.StructureToPtr (this, this_as_native, false);
			IntPtr native_rect = GLib.Marshaller.StructureToPtrAlloc (rect);
			pango_matrix_transform_rectangle(this_as_native, native_rect);
			ReadNative (this_as_native, ref this);
			System.Runtime.InteropServices.Marshal.FreeHGlobal (this_as_native);
			rect = Pango.Rectangle.New (native_rect);
			Marshal.FreeHGlobal (native_rect);
		}

		[DllImport("libpango-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void pango_matrix_translate(IntPtr raw, double tx, double ty);

		public void Translate(double tx, double ty) {
			IntPtr this_as_native = System.Runtime.InteropServices.Marshal.AllocHGlobal (System.Runtime.InteropServices.Marshal.SizeOf (this));
			System.Runtime.InteropServices.Marshal.StructureToPtr (this, this_as_native, false);
			pango_matrix_translate(this_as_native, tx, ty);
			ReadNative (this_as_native, ref this);
			System.Runtime.InteropServices.Marshal.FreeHGlobal (this_as_native);
		}

		static void ReadNative (IntPtr native, ref Pango.Matrix target)
		{
			target = New (native);
		}

		public bool Equals (Matrix other)
		{
			return true && Xx.Equals (other.Xx) && Xy.Equals (other.Xy) && Yx.Equals (other.Yx) && Yy.Equals (other.Yy) && X0.Equals (other.X0) && Y0.Equals (other.Y0);
		}

		public override bool Equals (object other)
		{
			return other is Matrix && Equals ((Matrix) other);
		}

		public override int GetHashCode ()
		{
			return this.GetType ().FullName.GetHashCode () ^ Xx.GetHashCode () ^ Xy.GetHashCode () ^ Yx.GetHashCode () ^ Yy.GetHashCode () ^ X0.GetHashCode () ^ Y0.GetHashCode ();
		}

		public static explicit operator GLib.Value (Pango.Matrix boxed)
		{
			GLib.Value val = GLib.Value.Empty;
			val.Init (Pango.Matrix.GType);
			val.Val = boxed;
			return val;
		}

		public static explicit operator Pango.Matrix (GLib.Value val)
		{
			return (Pango.Matrix) val.Val;
		}

		[Obsolete("This is a no-op")]
		public Pango.Matrix Copy() {
			return this;
		}
#endregion
	}
}
