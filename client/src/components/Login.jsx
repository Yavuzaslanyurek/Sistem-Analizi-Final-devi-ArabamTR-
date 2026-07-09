import React, { useState } from 'react';

export default function Login({ onViewChange, onLoginSuccess, on2FaRequired }) {
  const [formData, setFormData] = useState({ email: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(formData)
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data || 'Giriş yapılamadı. E-posta veya şifre hatalı.');
      }

      if (data.status === '2FA_GEREKLI') {
        // Redirect to 2FA view, passing the email and the test code
        on2FaRequired(formData.email, data.codeForTesting || '');
      } else {
        // Direct success
        localStorage.setItem('token', data.token);
        onLoginSuccess(data.user);
      }
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="w-full max-w-md p-8 rounded-3xl glass-panel relative overflow-hidden transition-all duration-300 hover:shadow-[0_0_50px_rgba(59,130,246,0.15)]">
      {/* Decorative neon light top-right */}
      <div className="absolute -top-10 -right-10 w-32 h-32 bg-blue-500/10 rounded-full blur-2xl"></div>

      <div className="text-center mb-8">
        <h2 className="text-3xl font-extrabold text-white tracking-tight">Giriş Yap</h2>
        <p className="text-slate-400 text-sm mt-2">Hesabınıza giriş yapın ve ilanları inceleyin</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-5">
        {error && (
          <div className="p-3 bg-red-500/10 border border-red-500/20 text-red-400 text-sm rounded-xl">
            {error}
          </div>
        )}

        <div>
          <label className="block text-slate-300 text-xs font-semibold uppercase tracking-wider mb-2">E-posta Adresi</label>
          <input
            type="email"
            required
            placeholder="ahmet@example.com"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            className="w-full bg-slate-900/60 border border-slate-800 text-white placeholder-slate-600 rounded-xl px-4 py-3 text-sm focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all duration-200"
          />
        </div>

        <div>
          <div className="flex justify-between items-center mb-2">
            <label className="block text-slate-300 text-xs font-semibold uppercase tracking-wider">Şifre</label>
            <button
              type="button"
              onClick={() => onViewChange('forgot-password')}
              className="text-blue-400 text-xs hover:underline hover:text-blue-300 focus:outline-none"
            >
              Şifremi Unuttum
            </button>
          </div>
          <input
            type="password"
            required
            placeholder="••••••••"
            value={formData.password}
            onChange={(e) => setFormData({ ...formData, password: e.target.value })}
            className="w-full bg-slate-900/60 border border-slate-800 text-white placeholder-slate-600 rounded-xl px-4 py-3 text-sm focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all duration-200"
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-gradient-to-r from-blue-600 to-blue-800 hover:from-blue-500 hover:to-blue-700 text-white font-bold py-3 rounded-xl shadow-lg hover:shadow-blue-500/20 active:scale-[0.98] transition-all duration-200 disabled:opacity-50"
        >
          {loading ? 'Giriş Yapılıyor...' : 'Giriş Yap'}
        </button>
      </form>

      <div className="mt-8 text-center text-slate-400 text-sm">
        Hesabınız yok mu?{' '}
        <button
          onClick={() => onViewChange('register')}
          className="text-blue-400 hover:underline hover:text-blue-300 font-semibold focus:outline-none"
        >
          Şimdi Kayıt Olun
        </button>
      </div>
    </div>
  );
}
