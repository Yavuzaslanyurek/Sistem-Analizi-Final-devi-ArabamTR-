import React, { useState } from 'react';

export default function Register({ onViewChange }) {
  const [formData, setFormData] = useState({ name: '', email: '', password: '', confirmPassword: '' });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (formData.password !== formData.confirmPassword) {
      setError('Şifreler eşleşmiyor.');
      return;
    }

    setLoading(true);

    try {
      const response = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: formData.name,
          email: formData.email,
          password: formData.password
        })
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data || 'Kayıt sırasında bir hata oluştu.');
      }

      setSuccess('Kayıt işlemi başarılı! Giriş sayfasına yönlendiriliyorsunuz...');
      setFormData({ name: '', email: '', password: '', confirmPassword: '' });
      
      setTimeout(() => {
        onViewChange('login');
      }, 2000);
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
        <h2 className="text-3xl font-extrabold text-white tracking-tight">Kayıt Ol</h2>
        <p className="text-slate-400 text-sm mt-2">ArabamTR dünyasına katılın ve ilan vermeye başlayın</p>
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
          <label className="block text-slate-300 text-xs font-semibold uppercase tracking-wider mb-2">Ad Soyad</label>
          <input
            type="text"
            required
            placeholder="Ahmet Yılmaz"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            className="w-full bg-slate-900/60 border border-slate-800 text-white placeholder-slate-600 rounded-xl px-4 py-3 text-sm focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all duration-200"
          />
        </div>

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
          <label className="block text-slate-300 text-xs font-semibold uppercase tracking-wider mb-2">Şifre</label>
          <input
            type="password"
            required
            placeholder="••••••••"
            value={formData.password}
            onChange={(e) => setFormData({ ...formData, password: e.target.value })}
            className="w-full bg-slate-900/60 border border-slate-800 text-white placeholder-slate-600 rounded-xl px-4 py-3 text-sm focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all duration-200"
          />
        </div>

        <div>
          <label className="block text-slate-300 text-xs font-semibold uppercase tracking-wider mb-2">Şifre Tekrarı</label>
          <input
            type="password"
            required
            placeholder="••••••••"
            value={formData.confirmPassword}
            onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
            className="w-full bg-slate-900/60 border border-slate-800 text-white placeholder-slate-600 rounded-xl px-4 py-3 text-sm focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all duration-200"
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-gradient-to-r from-blue-600 to-blue-800 hover:from-blue-500 hover:to-blue-700 text-white font-bold py-3 rounded-xl shadow-lg hover:shadow-blue-500/20 active:scale-[0.98] transition-all duration-200 disabled:opacity-50"
        >
          {loading ? 'Kayıt Yapılıyor...' : 'Kayıt Ol'}
        </button>
      </form>

      <div className="mt-8 text-center text-slate-400 text-sm">
        Zaten bir hesabınız var mı?{' '}
        <button
          onClick={() => onViewChange('login')}
          className="text-blue-400 hover:underline hover:text-blue-300 font-semibold focus:outline-none"
        >
          Giriş Yapın
        </button>
      </div>
    </div>
  );
}
