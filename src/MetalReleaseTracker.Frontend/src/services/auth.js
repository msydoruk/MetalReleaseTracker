import axios from 'axios';

// Support for different environments
const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:5001/api';

class AuthService {
  async loginWithEmail(email, password, rememberMe = false) {
    try {
      console.log('Starting login process with email...');
      
      // Clear old authentication data
      this.clearStaleState();
      
      const response = await axios.post(`${API_BASE_URL}/auth/login/email`, {
        email,
        password,
        rememberMe
      });
      
      if (response.data && response.data.success) {
        console.log('Email login successful');
        
        // Store the auth information
        this.storeUserData(response.data);
        
        return response.data;
      } else {
        console.error('Login failed:', response.data?.message || 'Unknown error');
        throw new Error(response.data?.message || 'Login failed');
      }
    } catch (error) {
      console.error('Login with email error:', error);
      throw error.response?.data?.message || error.message || 'Login failed';
    }
  }
  
  async register(email, password, confirmPassword, userName = '') {
    try {
      console.log('Starting registration process...');
      
      // Clear old authentication data
      this.clearStaleState();
      
      const response = await axios.post(`${API_BASE_URL}/auth/register`, {
        email,
        password,
        confirmPassword,
        userName: userName || email
      });
      
      if (response.data && response.data.success) {
        console.log('Registration successful');
        
        // Store the auth information
        this.storeUserData(response.data);
        
        return response.data;
      } else {
        console.error('Registration failed:', response.data?.message || 'Unknown error');
        throw new Error(response.data?.message || 'Registration failed');
      }
    } catch (error) {
      console.error('Registration error:', error);
      throw error.response?.data?.message || error.message || 'Registration failed';
    }
  }
  
  storeUserData(data) {
    // Store JWT token and user information
    localStorage.setItem('user_data', JSON.stringify(data));
    localStorage.setItem('login_timestamp', Date.now().toString());
    localStorage.setItem('auth_token', data.token);
    localStorage.setItem('refresh_token', data.refreshToken);
    localStorage.setItem('token_expiration', new Date(data.expiration).getTime().toString());
    
    if (data.claims) {
      localStorage.setItem('user_email', data.claims.email);
      localStorage.setItem('user_name', data.claims.username);
      localStorage.setItem('user_id', data.claims.id);
    }
    
    // Trigger auth update event
    this.triggerAuthUpdate();
  }
  
  clearStaleState() {
    try {
      console.log('Clearing auth state...');
      
      const storageKeys = [
        'user_data', 
        'login_timestamp',
        'auth_token',
        'refresh_token',
        'token_expiration',
        'user_email',
        'user_name',
        'user_id'
      ];
      
      storageKeys.forEach(key => {
        try {
          localStorage.removeItem(key);
        } catch (e) {
          console.error(`Error removing ${key} from localStorage:`, e);
        }
      });
    } catch (e) {
      console.error('Error clearing stale state:', e);
    }
  }

  // Logout with token revocation
  async logout() {
    try {
      const refreshToken = localStorage.getItem('refresh_token');
      const userId = localStorage.getItem('user_id');
      
      if (refreshToken && userId) {
        await axios.post(`${API_BASE_URL}/auth/revoke-token`, {
          refreshToken,
          userId
        });
      }
      
      this.clearStaleState();
      window.location.href = '/login';
    } catch (error) {
      console.error('Logout error:', error);
      this.clearStaleState();
      window.location.href = '/login';
    }
  }

  // Get token for API requests
  async getToken() {
    try {
      const token = localStorage.getItem('auth_token');
      const expirationTime = localStorage.getItem('token_expiration');
      
      if (!token || !expirationTime) {
        return null;
      }
      
      const now = Date.now();
      const expiry = parseInt(expirationTime, 10);
      
      // If token is about to expire (less than 5 minutes), try to refresh it
      if (expiry - now < 5 * 60 * 1000) {
        return this.refreshToken();
      }
      
      return token;
    } catch (error) {
      console.error('Error getting token:', error);
      return null;
    }
  }
  
  // Refresh the JWT token
  async refreshToken() {
    try {
      const refreshToken = localStorage.getItem('refresh_token');
      const userId = localStorage.getItem('user_id');
      
      if (!refreshToken || !userId) {
        // No refresh token or userId, user must login again
        this.clearStaleState();
        return null;
      }
      
      const response = await axios.post(`${API_BASE_URL}/auth/refresh-token`, {
        refreshToken,
        userId
      });
      
      if (response.data && response.data.success) {
        // Update stored tokens
        this.storeUserData(response.data);
        return response.data.token;
      } else {
        console.error('Token refresh failed:', response.data?.message);
        this.clearStaleState();
        return null;
      }
    } catch (error) {
      console.error('Error refreshing token:', error);
      this.clearStaleState();
      return null;
    }
  }

  // Get current user
  async getUser() {
    try {
      const userData = localStorage.getItem('user_data');
      
      if (userData) {
        return JSON.parse(userData);
      }
      
      return null;
    } catch (error) {
      console.error('Error getting user data:', error);
      return null;
    }
  }

  // Check if user is logged in
  async isLoggedIn() {
    try {
      const token = localStorage.getItem('auth_token');
      const expirationTime = localStorage.getItem('token_expiration');
      
      if (!token || !expirationTime) {
        return false;
      }
      
      const now = Date.now();
      const expiry = parseInt(expirationTime, 10);
      
      // If token is expired
      if (now >= expiry) {
        // Try to refresh the token
        const newToken = await this.refreshToken();
        return !!newToken;
      }
      
      return true;
    } catch (error) {
      console.error('Error checking login status:', error);
      return false;
    }
  }

  // Trigger authentication state update
  triggerAuthUpdate() {
    // Create and dispatch a custom event that components can listen for
    const event = new CustomEvent('auth_state_changed', {
      detail: { timestamp: Date.now() }
    });
    window.dispatchEvent(event);
  }

  async loginWithGoogle() {
    return new Promise((resolve, reject) => {
      const returnUrl = `${window.location.origin}/auth/callback`;
      
      const popup = window.open(
        `${API_BASE_URL}/auth/google-login?returnUrl=${encodeURIComponent(returnUrl)}`,
        'googleLogin',
        'width=500,height=600,scrollbars=yes,resizable=yes'
      );

      const checkClosed = setInterval(() => {
        if (popup.closed) {
          clearInterval(checkClosed);
          reject(new Error('Login was cancelled by user'));
        }
      }, 1000);

      const handleMessage = (event) => {
        // Verify origin for security
        const frontendUrl = window.location.origin;
        if (event.origin !== frontendUrl) {
          return;
        }

        if (event.data.type === 'GOOGLE_AUTH_SUCCESS') {
          clearInterval(checkClosed);
          popup.close();
          window.removeEventListener('message', handleMessage);
          
          // Store the auth data
          this.storeUserData(event.data);
          resolve(event.data);
        } else if (event.data.type === 'GOOGLE_AUTH_ERROR') {
          clearInterval(checkClosed);
          popup.close();
          window.removeEventListener('message', handleMessage);
          reject(new Error(event.data.error || 'Google authentication failed'));
        }
      };

      window.addEventListener('message', handleMessage);
    });
  }

  handleGoogleCallback() {
    try {
      const urlParams = new URLSearchParams(window.location.search);
      const token = urlParams.get('token');
      const refreshToken = urlParams.get('refreshToken');
      const error = urlParams.get('error');
      
      if (token && refreshToken) {
        const authData = {
          success: true,
          token: decodeURIComponent(token),
          refreshToken: decodeURIComponent(refreshToken),
          claims: this.parseJWTClaims(decodeURIComponent(token)),
          expiration: new Date(Date.now() + 3600000).toISOString() // 1 hour from now
        };
        
        // Send success message to parent window
        if (window.opener) {
          window.opener.postMessage({
            type: 'GOOGLE_AUTH_SUCCESS',
            ...authData
          }, window.location.origin);
          window.close();
        } else {
          // Direct navigation (not popup)
          this.storeUserData(authData);
          window.location.href = '/';
        }
      } else if (error) {
        const errorMessage = decodeURIComponent(error);
        if (window.opener) {
          window.opener.postMessage({
            type: 'GOOGLE_AUTH_ERROR',
            error: errorMessage
          }, window.location.origin);
          window.close();
        } else {
          window.location.href = '/login?error=' + encodeURIComponent(errorMessage);
        }
      } else {
        const errorMsg = 'Missing authentication tokens';
        if (window.opener) {
          window.opener.postMessage({
            type: 'GOOGLE_AUTH_ERROR',
            error: errorMsg
          }, window.location.origin);
          window.close();
        } else {
          window.location.href = '/login?error=' + encodeURIComponent(errorMsg);
        }
      }
    } catch (error) {
      console.error('Google callback error:', error);
      if (window.opener) {
        window.opener.postMessage({
          type: 'GOOGLE_AUTH_ERROR',
          error: error.message
        }, window.location.origin);
        window.close();
      } else {
        window.location.href = '/login?error=' + encodeURIComponent(error.message);
      }
    }
  }

  parseJWTClaims(token) {
    try {
      const payload = token.split('.')[1];
      const decoded = JSON.parse(atob(payload));
      return {
        id: decoded.sub || decoded.nameid,
        email: decoded.email || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
        username: decoded.name || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || decoded.email
      };
    } catch (error) {
      console.error('Error parsing JWT claims:', error);
      return {};
    }
  }
}

const authService = new AuthService();
export default authService;