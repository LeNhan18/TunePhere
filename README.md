# TunePhere

#Mô tả hệ thống TuneSphere

TuneSphere là một nền tảng phát nhạc trực tuyến được phát triển bằng ASP.NET Core, tích hợp các công nghệ hiện đại để mang đến trải nghiệm âm nhạc cá nhân hóa, sáng tạo và tương tác xã hội. Hệ thống cho phép người dùng nghe nhạc, tạo và remix bài hát, tham gia phòng nghe nhạc ảo, đồng thời hiển thị lời bài hát đồng bộ theo thời gian thực. Với mục tiêu kết nối cộng đồng yêu nhạc, TuneSphere không chỉ là một ứng dụng nghe nhạc mà còn là không gian để sáng tạo và chia sẻ.

# Các thành phần chính:

# Hệ thống phát nhạc và cá nhân hóa:
Cung cấp thư viện nhạc phong phú, hỗ trợ tìm kiếm theo từ khóa, nghệ sĩ hoặc thể loại.
Sử dụng AI để phân tích sở thích người dùng (dựa trên lịch sử nghe nhạc) và tạo playlist tự động.
Phát nhạc qua Web Audio API với chất lượng cao, lưu trữ file trên Azure Blob Storage.
# Sáng tạo và remix nhạc:
Người dùng có thể remix bài hát gốc bằng công cụ kéo-thả, thêm hiệu ứng âm thanh hoặc thay đổi nhịp độ.
AI (tích hợp Azure AI hoặc Magenta) gợi ý hòa âm và phối khí, giúp người dùng tạo bản nhạc độc đáo.
Bản remix được lưu và chia sẻ với cộng đồng, hỗ trợ tải xuống dạng MP3.
# Playlist cộng đồng:
Người dùng tạo playlist cá nhân hoặc công khai, mời người khác cùng thêm bài hát và bình chọn nội dung.
Hệ thống quản lý các phiên bản playlist qua cơ sở dữ liệu SQL Server, đảm bảo tính linh hoạt và dễ truy cập.
# Phòng nghe nhạc ảo:
Tạo không gian nghe nhạc đồng bộ giữa nhiều người dùng trong thời gian thực, tích hợp chat và bỏ phiếu bài hát.
SignalR đảm bảo cập nhật tức thì về bài hát hiện tại và tương tác giữa các thành viên.
Hiển thị lời bài hát đồng bộ:
Hiển thị lyrics chạy theo nhạc với timestamp chính xác, làm nổi bật dòng hiện tại bằng hiệu ứng giao diện.
Dữ liệu lyrics được lấy từ cơ sở dữ liệu hoặc API bên ngoài (Musixmatch), đồng bộ trong phòng nghe nhạc qua SignalR.
# Công nghệ sử dụng:
Backend: ASP.NET Core (API/MVC) xử lý logic chính, tích hợp Entity Framework Core để truy vấn SQL Server.
Frontend: Blazor Server/WebAssembly tạo giao diện tương tác, Web Audio API xử lý phát nhạc và remix.
Cơ sở dữ liệu: SQL Server lưu trữ thông tin người dùng, bài hát, playlist, remix và lyrics.
Lưu trữ: Azure Blob Storage lưu file nhạc, remix và lyrics; Azure CDN tối ưu tốc độ tải.
Thời gian thực: SignalR đồng bộ phòng nghe nhạc và lyrics.
AI: Azure AI hoặc Magenta hỗ trợ gợi ý nhạc và sáng tác.
Bảo mật: ASP.NET Identity quản lý đăng nhập, mã hóa dữ liệu nhạy cảm (lyrics, file nhạc).
# Kiến trúc hệ thống:
Client-Server: Người dùng truy cập qua trình duyệt, giao tiếp với backend qua RESTful API.
Layered Architecture: Gồm Presentation Layer (Blazor), Business Logic Layer (ASP.NET Core), và Data Access Layer (Entity Framework).
Scalability: Hỗ trợ mở rộng bằng Azure Cloud, caching bằng Redis cho truy vấn nhanh.
# Mục tiêu và giá trị:
Mang đến trải nghiệm nghe nhạc cá nhân hóa với playlist thông minh và lyrics đồng bộ.
Khuyến khích sáng tạo âm nhạc qua công cụ remix và hợp tác cộng đồng.
Tăng tính tương tác xã hội qua phòng nghe nhạc ảo và chia sẻ nội dung.
Đảm bảo hiệu suất cao, bảo mật dữ liệu, và khả năng mở rộng cho hàng triệu người dùng.
