//
// Copyright (c) 2014 Citrix Systems, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

/// What a pain in the arse, that these don't exist.
/// <summary>
/// No-op TextWriter implementation.
/// </summary>
class TextWriterNull : System.IO.TextWriter
{
    public override System.Text.Encoding Encoding
    {
        get { return System.Text.Encoding.Default; }
    }

    public override void Close()
    {
        base.Close();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    public override void Flush()
    {
    }

    public override void Write(bool value)
    {
    }

    public override void Write(char value)
    {
    }

    public override void Write(char[] buffer)
    {
    }

    public override void Write(decimal value)
    {
    }

    public override void Write(double value)
    {
    }

    public override void Write(float value)
    {
    }

    public override void Write(int value)
    {
    }

    public override void Write(long value)
    {
    }

    public override void Write(object value)
    {
    }

    public override void Write(string value)
    {
    }

    public override void Write(uint value)
    {
    }

    public override void Write(ulong value)
    {
    }

    public override void Write(string format, object arg0)
    {
    }

    public override void Write(string format, params object[] arg)
    {
    }

    public override void Write(char[] buffer, int index, int count)
    {
    }

    public override void Write(string format, object arg0, object arg1)
    {
    }

    public override void Write(string format, object arg0, object arg1, object arg2)
    {
    }

    public override void WriteLine()
    {
    }

    public override void WriteLine(bool value)
    {
    }

    public override void WriteLine(char value)
    {
    }

    public override void WriteLine(char[] buffer)
    {
    }

    public override void WriteLine(decimal value)
    {
    }

    public override void WriteLine(double value)
    {
    }

    public override void WriteLine(float value)
    {
    }

    public override void WriteLine(int value)
    {
    }

    public override void WriteLine(long value)
    {
    }

    public override void WriteLine(object value)
    {
    }

    public override void WriteLine(string value)
    {
    }

    public override void WriteLine(uint value)
    {
    }

    public override void WriteLine(ulong value)
    {
    }

    public override void WriteLine(string format, object arg0)
    {
    }

    public override void WriteLine(string format, params object[] arg)
    {
    }

    public override void WriteLine(char[] buffer, int index, int count)
    {
    }

    public override void WriteLine(string format, object arg0, object arg1)
    {
    }

    public override void WriteLine(string format, object arg0, object arg1, object arg2)
    {
    }

}

/// <summary>
/// TextWriter wrapper for System.Diagnostics.Debug.
/// </summary>
class TextWriterDebug : System.IO.TextWriter
{
    public override System.Text.Encoding Encoding
    {
        get { return System.Text.Encoding.Default; }
    }

    public override void Close()
    {
        System.Diagnostics.Debug.Close();
        base.Close();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    public override void Flush()
    {
        System.Diagnostics.Debug.Flush();
        base.Flush();
    }

    public override void Write(bool value)
    {
        System.Diagnostics.Debug.Write(value);
    }

    public override void Write(char value)
    {
        System.Diagnostics.Debug.Write(value);
    }

    public override void Write(char[] buffer)
    {
        System.Diagnostics.Debug.Write(buffer);
    }

    public override void Write(decimal value)
    {
        System.Diagnostics.Debug.Write(value);
    }

    public override void Write(double value)
    {
        System.Diagnostics.Debug.Write(value);
    }

    public override void Write(float value)
    {
        System.Diagnostics.Debug.Write(value);
    }

    public override void Write(int value)
    {
        System.Diagnostics.Debug.Write(value);
    }

    public override void Write(long value)
    {
        System.Diagnostics.Debug.Write(value);
    }

    public override void Write(object value)
    {
        System.Diagnostics.Debug.Write(value);
    }

    public override void Write(string value)
    {
        System.Diagnostics.Debug.Write(value);
    }

    public override void Write(uint value)
    {
        System.Diagnostics.Debug.Write(value);
    }

    public override void Write(ulong value)
    {
        System.Diagnostics.Debug.Write(value);
    }

    public override void Write(string format, object arg0)
    {
        System.Diagnostics.Debug.Write(string.Format(format, arg0));
    }

    public override void Write(string format, params object[] arg)
    {
        System.Diagnostics.Debug.Write(string.Format(format, arg));
    }

    public override void Write(char[] buffer, int index, int count)
    {
        string x = new string(buffer, index, count);
        System.Diagnostics.Debug.Write(x);
    }

    public override void Write(string format, object arg0, object arg1)
    {
        System.Diagnostics.Debug.Write(string.Format(format, arg0, arg1));
    }

    public override void Write(string format, object arg0, object arg1, object arg2)
    {
        System.Diagnostics.Debug.Write(string.Format(format, arg0, arg1, arg2));
    }

    public override void WriteLine()
    {
        System.Diagnostics.Debug.WriteLine(string.Empty);
    }

    public override void WriteLine(bool value)
    {
        System.Diagnostics.Debug.WriteLine(value);
    }

    public override void WriteLine(char value)
    {
        System.Diagnostics.Debug.WriteLine(value);
    }

    public override void WriteLine(char[] buffer)
    {
        System.Diagnostics.Debug.WriteLine(buffer);
    }

    public override void WriteLine(decimal value)
    {
        System.Diagnostics.Debug.WriteLine(value);
    }

    public override void WriteLine(double value)
    {
        System.Diagnostics.Debug.WriteLine(value);
    }

    public override void WriteLine(float value)
    {
        System.Diagnostics.Debug.WriteLine(value);
    }

    public override void WriteLine(int value)
    {
        System.Diagnostics.Debug.WriteLine(value);
    }

    public override void WriteLine(long value)
    {
        System.Diagnostics.Debug.WriteLine(value);
    }

    public override void WriteLine(object value)
    {
        System.Diagnostics.Debug.WriteLine(value);
    }

    public override void WriteLine(string value)
    {
        System.Diagnostics.Debug.WriteLine(value);
    }

    public override void WriteLine(uint value)
    {
        System.Diagnostics.Debug.WriteLine(value);
    }

    public override void WriteLine(ulong value)
    {
        System.Diagnostics.Debug.WriteLine(value);
    }

    public override void WriteLine(string format, object arg0)
    {
        System.Diagnostics.Debug.WriteLine(string.Format(format, arg0));
    }

    public override void WriteLine(string format, params object[] arg)
    {
        System.Diagnostics.Debug.WriteLine(string.Format(format, arg));
    }

    public override void WriteLine(char[] buffer, int index, int count)
    {
        string x = new string(buffer, index, count);
        System.Diagnostics.Debug.WriteLine(x);

    }

    public override void WriteLine(string format, object arg0, object arg1)
    {
        System.Diagnostics.Debug.WriteLine(string.Format(format, arg0, arg1));
    }

    public override void WriteLine(string format, object arg0, object arg1, object arg2)
    {
        System.Diagnostics.Debug.WriteLine(string.Format(format, arg0, arg1, arg2));
    }

} // Ends class TextWriterDebug 

/// <summary>
/// Spreads data out to multiple text writers.
/// </summary>
class TextWriterMulti : System.IO.TextWriter
{
    private System.Collections.Generic.List<System.IO.TextWriter> writers = new System.Collections.Generic.List<System.IO.TextWriter>();
    private System.IFormatProvider formatProvider = null;
    private System.Text.Encoding encoding = null;

    #region TextWriter Properties
    public override System.IFormatProvider FormatProvider
    {
        get
        {
            System.IFormatProvider formatProvider = this.formatProvider;
            if (formatProvider == null)
            {
                formatProvider = base.FormatProvider;
            }
            return formatProvider;
        }
    }

    public override string NewLine
    {
        get { return base.NewLine; }

        set
        {
            foreach (System.IO.TextWriter writer in this.writers)
            {
                writer.NewLine = value;
            }

            base.NewLine = value;
        }
    }


    public override System.Text.Encoding Encoding
    {
        get
        {
            System.Text.Encoding encoding = this.encoding;

            if (encoding == null)
            {
                encoding = System.Text.Encoding.Default;
            }

            return encoding;
        }
    }

    #region TextWriter Property Setters

    TextWriterMulti SetFormatProvider(System.IFormatProvider value)
    {
        this.formatProvider = value;
        return this;
    }

    TextWriterMulti SetEncoding(System.Text.Encoding value)
    {
        this.encoding = value;
        return this;
    }
    #endregion // TextWriter Property Setters
    #endregion // TextWriter Properties


    #region Construction/Destruction
    public TextWriterMulti(params System.IO.TextWriter[] writers)
        : this((System.Collections.Generic.IEnumerable<System.IO.TextWriter>) writers)
    {
    }

    public TextWriterMulti(System.Collections.Generic.IEnumerable<System.IO.TextWriter> writers)
    {
        this.Clear();
        this.AddWriters(writers);
    }
    #endregion // Construction/Destruction

    #region Public interface
    public TextWriterMulti Clear()
    {
        this.writers.Clear();
        return this;
    }

    public TextWriterMulti AddWriter(System.IO.TextWriter writer)
    {
        this.writers.Add(writer);
        return this;
    }

    public TextWriterMulti AddWriters(System.Collections.Generic.IEnumerable<System.IO.TextWriter> writers)
    {
        this.writers.AddRange(writers);
        return this;
    }
    #endregion // Public interface

    #region TextWriter methods

    public override void Close()
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Close();
        }
        base.Close();
    }

    protected override void Dispose(bool disposing)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            if (disposing)
            {
                writer.Dispose();
            }
        }
        base.Dispose(disposing);
    }

    public override void Flush()
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Flush();
        }

        base.Flush();
    }

    //foreach (System.IO.TextWriter writer in this.writers)
    //{
    //    writer;
    //}
    public override void Write(bool value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(value);
        }
    }

    public override void Write(char value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(value);
        }
    }

    public override void Write(char[] buffer)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(buffer);
        }
    }

    public override void Write(decimal value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(value);
        }
    }

    public override void Write(double value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(value);
        }
    }

    public override void Write(float value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(value);
        }
    }

    public override void Write(int value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(value);
        }
    }

    public override void Write(long value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(value);
        }
    }

    public override void Write(object value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(value);
        }
    }

    public override void Write(string value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(value);
        }
    }

    public override void Write(uint value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(value);
        }
    }

    public override void Write(ulong value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(value);
        }
    }

    public override void Write(string format, object arg0)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(format, arg0);
        }

    }

    public override void Write(string format, params object[] arg)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(format, arg);
        }
    }

    public override void Write(char[] buffer, int index, int count)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(buffer, index, count);
        }
    }

    public override void Write(string format, object arg0, object arg1)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(format, arg0, arg1);
        }
    }

    public override void Write(string format, object arg0, object arg1, object arg2)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.Write(format, arg0, arg1, arg2);
        }
    }

    public override void WriteLine()
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine();
        }
    }

    public override void WriteLine(bool value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    public override void WriteLine(char value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    public override void WriteLine(char[] buffer)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(buffer);
        }
    }

    public override void WriteLine(decimal value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    public override void WriteLine(double value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    public override void WriteLine(float value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
           writer.WriteLine(value);
        }
    }

    public override void WriteLine(int value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    public override void WriteLine(long value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    public override void WriteLine(object value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    public override void WriteLine(string value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    public override void WriteLine(uint value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    public override void WriteLine(ulong value)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    public override void WriteLine(string format, object arg0)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(format, arg0);
        }
    }

    public override void WriteLine(string format, params object[] arg)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(format, arg);
        }
    }

    public override void WriteLine(char[] buffer, int index, int count)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(buffer, index, count);
        }
    }

    public override void WriteLine(string format, object arg0, object arg1)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(format, arg0, arg1);
        }
    }

    public override void WriteLine(string format, object arg0, object arg1, object arg2)
    {
        foreach (System.IO.TextWriter writer in this.writers)
        {
            writer.WriteLine(format, arg0, arg1, arg2);
        }
    }
    #endregion // TextWriter methods
}
