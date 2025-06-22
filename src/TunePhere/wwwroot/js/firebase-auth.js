// Firebase configuration
const firebaseConfig = {
    apiKey: "AIzaSyA91Rb17rmaKPtjK4o_qOaQeH_iaEyd6vE",
    authDomain: "tunephere.firebaseapp.com",
    projectId: "tunephere",
    storageBucket: "tunephere.appspot.com",
    messagingSenderId: "745535728894",
    appId: "1:745535728894:web:a48e3a54954539939c89af",
    measurementId: "G-WE5RDVYK22"
};

// Initialize Firebase
firebase.initializeApp(firebaseConfig);
const analytics = firebase.analytics();
const auth = firebase.auth();

// Xử lý đăng nhập bằng Google
async function signInWithGoogle() {
    try {
        const provider = new firebase.auth.GoogleAuthProvider();
        // Thêm scopes nếu cần
        provider.addScope('email');
        provider.addScope('profile');
        
        const result = await auth.signInWithPopup(provider);
        console.log("Đăng nhập Google thành công:", result.user);
        
        // Lấy ID token
        const idToken = await result.user.getIdToken();
        console.log("Đã lấy được ID token từ Google");
        
        // Gửi token lên server
        const response = await fetch('/FirebaseAuth/Login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ idToken })
        });

        const data = await response.json();
        console.log("Server response:", data);
        
        if (data.success) {
            // Chuyển hướng về trang chủ sau khi đăng nhập thành công
            window.location.href = '/';
        } else {
            alert('Đăng nhập thất bại: ' + data.message);
        }
    } catch (error) {
        console.error('Chi tiết lỗi Google:', error);
        let errorMessage = 'Có lỗi xảy ra khi đăng nhập Google: ';
        
        switch (error.code) {
            case 'auth/popup-closed-by-user':
                errorMessage += 'Cửa sổ đăng nhập đã bị đóng. Vui lòng thử lại.';
                break;
            case 'auth/cancelled-popup-request':
                errorMessage += 'Yêu cầu đăng nhập đã bị hủy. Vui lòng thử lại.';
                break;
            case 'auth/popup-blocked':
                errorMessage += 'Cửa sổ đăng nhập bị chặn. Vui lòng cho phép popup và thử lại.';
                break;
            case 'auth/configuration-not-found':
                errorMessage += 'Cấu hình Firebase không đúng. Vui lòng kiểm tra lại cấu hình.';
                break;
            default:
                errorMessage += error.message;
        }
        
        alert(errorMessage);
    }
}

// Xử lý đăng nhập bằng Facebook
async function signInWithFacebook() {
    try {
        const provider = new firebase.auth.FacebookAuthProvider();
        // Thêm scopes nếu cần
        provider.addScope('email');
        provider.addScope('public_profile');

        const result = await auth.signInWithPopup(provider);
        console.log("Đăng nhập Facebook thành công:", result.user);

        // Lấy ID token
        const idToken = await result.user.getIdToken();
        console.log("Đã lấy được ID token từ Facebook");

        // Gửi token lên server
        const response = await fetch('/FirebaseAuth/Login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ idToken })
        });

        const data = await response.json();
        console.log("Server response:", data);

        if (data.success) {
            // Chuyển hướng về trang chủ sau khi đăng nhập thành công
            window.location.href = '/';
        } else {
            alert('Đăng nhập Facebook thất bại: ' + data.message);
        }
    } catch (error) {
        console.error('Chi tiết lỗi Facebook:', error);
        let errorMessage = 'Có lỗi xảy ra khi đăng nhập Facebook: ';
        
        switch (error.code) {
            case 'auth/popup-closed-by-user':
                errorMessage += 'Cửa sổ đăng nhập đã bị đóng. Vui lòng thử lại.';
                break;
            case 'auth/cancelled-popup-request':
                errorMessage += 'Yêu cầu đăng nhập đã bị hủy. Vui lòng thử lại.';
                break;
            case 'auth/popup-blocked':
                errorMessage += 'Cửa sổ đăng nhập bị chặn. Vui lòng cho phép popup và thử lại.';
                break;
            case 'auth/configuration-not-found':
                errorMessage += 'Cấu hình Firebase không đúng. Vui lòng kiểm tra lại cấu hình.';
                break;
            default:
                errorMessage += error.message;
        }
        
        alert(errorMessage);
    }
}

// Thêm sự kiện click cho nút đăng nhập Google và Facebook
document.addEventListener('DOMContentLoaded', function() {
    const googleButtons = document.querySelectorAll('.social-icon.google');
    googleButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            signInWithGoogle();
        });
    });

    const facebookButtons = document.querySelectorAll('.social-icon.facebook');
    facebookButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            signInWithFacebook();
        });
    });
}); 