using System.Windows;
using System.Windows.Media;
using System.Security.Cryptography;
using System.Text;

namespace PointsOnline
{
    static class Extensions
    {
        public static UIElement FindByUid(this DependencyObject parent, string uid)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            if (count == 0) return null;

            for (int i = 0; i < count; i++)
            {
                var el = VisualTreeHelper.GetChild(parent, i) as UIElement;
                if (el == null) continue;

                if (el.Uid == uid) return el;

                el = el.FindByUid(uid);
                if (el != null) return el;
            }
            return null;
        }
    }

    static class Utils
    {
        public static string GetMd5(string s)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(s));
            StringBuilder sBuilder = new StringBuilder(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}
