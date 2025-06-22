using BussinessObject.Entity;

namespace ConferenceFWebAPI.Service
{
    public static class ConferenceCreatedTemplate
    {
        public static string GetHtml(Conference conference)
        {
            return $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; color: #333; }}
                .container {{ padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
                .header {{ background-color: #007bff; color: white; padding: 10px; border-radius: 5px 5px 0 0; }}
                .content {{ padding: 20px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>🎉 Hội thảo mới đã được tạo!</h2>
                </div>
                <div class='content'>
                    <p><strong>Tiêu đề:</strong> {conference.Title}</p>
                    <p><strong>Mô tả:</strong> {conference.Description}</p>
                    <p><strong>Địa điểm:</strong> {conference.Location}</p>
                    <p><strong>Thời gian:</strong> {conference.StartDate:dd/MM/yyyy} - {conference.EndDate:dd/MM/yyyy}</p>
                    <p><strong>Trạng thái:</strong> {conference.Status}</p>
                </div>
            </div>
        </body>
        </html>";
        }
    }

}
