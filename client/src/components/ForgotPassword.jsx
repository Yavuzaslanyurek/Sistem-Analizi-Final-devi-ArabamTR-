import React, { useState } from 'react';

export default function ForgotPassword({ onViewChange, onForgotPasswordSuccess }) {
  const [email, setEmail] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [testToken, setTestToken] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setTestToken('');
    setLoading(true);

    try {
      const response = await fetch('/api/auth/forgot-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email })
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data || 'Şifre sıfırlama talebi gönderilemedi.');
      }

      setSuccess('Şifre sıfırlama kodu başarıyla oluşturuldu.');
      if (data.tokenForTesting) {
        setTestToken(data.tokenForTesting);
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
        <h2 className="text-3xl font-extrabold text-white tracking-tight">Şifremi Unuttum</h2>
        <p className="text-slate-400 text-sm mt-2">Hesabınızın e-posta adresini girerek şifrenizi sıfırlayın</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-5">
        {error && (
          <div className="p-3 bg-red-500/10 border border-red-500/20 text-red-400 text-sm rounded-xl">
            {error}
          </div>
        )}
        {success && (
          <div className="p-3 bg-green-500/10 border border-green-500/20 text-green-400 text-sm rounded-xl">
            {success}
          </div>
        )}

        <div>
          <label className="block text-slate-300 text-xs font-semibold uppercase tracking-wider mb-2">E-posta Adresi</label>
          <input
            type="email"
            required
            placeholder="ahmet@example.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="w-full bg-slate-900/60 border border-slate-800 text-white placeholder-slate-600 rounded-xl px-4 py-3 text-sm focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all duration-200"
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-gradient-to-r from-blue-600 to-blue-800 hover:from-blue-500 hover:to-blue-700 text-white font-bold py-3 rounded-xl shadow-lg hover:shadow-blue-500/20 active:scale-[0.98] transition-all duration-200 disabled:opacity-50"
        >
          {loading ? 'İstek Gönderiliyor...' : 'Sıfırlama Kodu Al'}
        </button>
      </form>

      {testToken && (
        <div className="mt-6 p-4 bg-blue-500/10 border border-blue-500/20 rounded-2xl text-center">
          <span className="block text-blue-400 text-xs font-semibold uppercase tracking-wider mb-1">Geliştirici Test Kodu (Token)</span>
          <span className="text-sm font-mono break-all text-white block mb-3">{testToken}</span>
          <button
            onClick={() => onForgotPasswordSuccess(email, testToken)}
            className="bg-blue-600 hover:bg-blue-500 text-white text-xs font-bold px-4 py-2 rounded-lg transition-all duration-200"
          >
            Şifreyi Sıfırla Ekranına Git
          </button>
        </div>
      )}

      <div className="mt-8 text-center">
        <button
          onClick={() => onViewChange('login')}
          className="text-slate-400 hover:text-white text-sm font-semibold focus:outline-none transition-colors duration-200"
        >
          Giriş Sayfasına Geri Dön
        </button>
      </div>
    </div>
  );
}
