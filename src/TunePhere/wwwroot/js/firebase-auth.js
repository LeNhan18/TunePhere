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
// Analytics can fail in local/dev browsers and should not block auth flow.
if (typeof firebase.analytics === 'function') {
    try {
        firebase.analytics();
    } catch (e) {
        console.warn('Firebase Analytics init failed:', e?.message || e);
    }
}
const auth = firebase.auth();

function getFirebaseAuthErrorMessage(error, providerName) {
    const base = `Co loi xay ra khi dang nhap ${providerName}: `;

    switch (error?.code) {
        case 'auth/popup-closed-by-user':
            return base + 'Cua so dang nhap da bi dong. Vui long thu lai.';
        case 'auth/cancelled-popup-request':
            return base + 'Yeu cau dang nhap da bi huy. Vui long thu lai.';
        case 'auth/popup-blocked':
            return base + 'Trinh duyet da chan popup. Hay cho phep popup va thu lai.';
        case 'auth/configuration-not-found':
        case 'auth/operation-not-allowed':
            return base + 'Provider chua duoc bat tren Firebase Console (Authentication > Sign-in method).';
        case 'auth/unauthorized-domain':
            return base + 'Domain hien tai chua duoc them vao Authorized domains tren Firebase.';
        case 'auth/invalid-api-key':
        case 'auth/app-not-authorized':
            return base + 'Firebase config sai hoac project khong duoc phep su dung API key nay.';
        default:
            return base + (error?.message || 'Loi khong xac dinh. Vui long kiem tra Console.');
    }
}

async function postFirebaseToken(idToken) {
    const response = await fetch('/FirebaseAuth/Login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ idToken })
    });

    let data = null;
    try {
        data = await response.json();
    } catch {
        throw new Error('Server tra ve du lieu khong hop le.');
    }

    if (!response.ok || !data?.success) {
        throw new Error(data?.message || `Dang nhap that bai (HTTP ${response.status}).`);
    }

    return data;
}

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

        const data = await postFirebaseToken(idToken);
        console.log("Server response:", data);

        // Chuyển hướng về trang chủ sau khi đăng nhập thành công
        window.location.href = '/';
    } catch (error) {
        console.error('Chi tiết lỗi Google:', error);
        alert(getFirebaseAuthErrorMessage(error, 'Google'));
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

        const data = await postFirebaseToken(idToken);
        console.log("Server response:", data);

        // Chuyển hướng về trang chủ sau khi đăng nhập thành công
        window.location.href = '/';
    } catch (error) {
        console.error('Chi tiết lỗi Facebook:', error);
        alert(getFirebaseAuthErrorMessage(error, 'Facebook'));
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