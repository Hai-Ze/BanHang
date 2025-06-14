using System.Text.Json;

namespace BanHang.Extensions
{
    public static class SessionExtensions
    {
        /// <summary>
        /// Lưu object vào session dưới dạng JSON
        /// </summary>
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            var json = JsonSerializer.Serialize(value);
            session.SetString(key, json);
        }

        /// <summary>
        /// Lấy object từ session và deserialize từ JSON
        /// </summary>
        public static T? GetObjectFromJson<T>(this ISession session, string key) where T : class
        {
            var json = session.GetString(key);

            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (JsonException)
            {
                // Nếu không deserialize được, trả về null
                return null;
            }
        }

        /// <summary>
        /// Kiểm tra key có tồn tại trong session không
        /// </summary>
        public static bool HasKey(this ISession session, string key)
        {
            return session.Keys.Contains(key);
        }

        /// <summary>
        /// Xóa key khỏi session
        /// </summary>
        public static void RemoveKey(this ISession session, string key)
        {
            session.Remove(key);
        }
    }
}