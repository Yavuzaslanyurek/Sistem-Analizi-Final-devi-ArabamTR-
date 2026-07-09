import React, { useState } from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';

// Sahte Grafik Verisi Bileşeni (Basit SVG Grafik)
const PriceTrendChart = ({ modelId }) => {
    return (
        <div className="bg-gray-800 p-4 rounded-xl border border-gray-700 mt-6">
            <h4 className="text-sm font-semibold text-gray-400 mb-3">📉 Son 12 Aylık Fiyat Trendi (Model #{modelId || 1})</h4>
            <div className="h-32 flex items-end justify-between gap-2 pt-4 px-2 bg-gray-900 rounded-lg border border-gray-800">
                <div className="w-full flex flex-col items-center gap-1"><div className="w-full bg-blue-500 rounded-t h-16"></div><span className="text-[10px] text-gray-500">Oca</span></div>
                <div className="w-full flex flex-col items-center gap-1"><div className="w-full bg-blue-500 rounded-t h-20"></div><span className="text-[10px] text-gray-500">Mar</span></div>
                <div className="w-full flex flex-col items-center gap-1"><div className="w-full bg-blue-500 rounded-t h-24"></div><span className="text-[10px] text-gray-500">May</span></div>
                <div className="w-full flex flex-col items-center gap-1"><div className="w-full bg-indigo-500 rounded-t h-28"></div><span className="text-[10px] text-gray-500">Tem</span></div>
                <div className="w-full flex flex-col items-center gap-1"><div className="w-full bg-indigo-500 rounded-t h-26"></div><span className="text-[10px] text-gray-500">Eyl</span></div>
                <div className="w-full flex flex-col items-center gap-1"><div className="w-full bg-emerald-500 rounded-t h-32"></div><span className="text-[10px] text-gray-500">Kas/2026</span></div>
            </div>
        </div>
    );
};

function App() {
    const [selectedFeatures, setSelectedFeatures] = useState([]);
    const [selectedVehicle, setSelectedVehicle] = useState(null);
    const [plateQuery, setPlateQuery] = useState('');
    const [historyResult, setHistoryResult] = useState(null);
    const [messages, setMessages] = useState([
        { id: 1, sender: 'Ahmet Yılmaz', text: 'Araçta boya var mı?', time: '19:30' },
        { id: 2, sender: 'Sen', text: 'Sadece sağ çamurluk lokal boyalı.', time: '19:32' }
    ]);
    const [newMessage, setNewMessage] = useState('');

    const availableFeatures = [
        { id: 1, name: 'Sunroof' },
        { id: 2, name: 'Koltuk Isıtma' },
        { id: 3, name: 'Şerit Takip' },
        { id: 4, name: 'Elektrikli Bagaj' },
        { id: 5, name: 'Hayalet Gösterge' }
    ];

    const initialVehicles = [
        { id: 101, title: 'Volkswagen Golf 1.5 eTSI Style', price: '1.450.000 TL', km: '45.000', year: 2024, modelId: 1, plate: '34ABC123', aiBadge: 'Normal Fiyat', aiColor: 'bg-blue-500/20 text-blue-400', features: [1, 2, 5] },
        { id: 102, title: 'BMW 320i Sedan M Sport', price: '2.850.000 TL', km: '12.000', year: 2025, modelId: 2, plate: '06XYZ999', aiBadge: 'Fiyatı Çok İyi', aiColor: 'bg-emerald-500/20 text-emerald-400', features: [1, 2, 3, 4, 5] },
        { id: 103, title: 'Toyota Corolla 1.8 Hybrid Dream', price: '1.180.000 TL', km: '85.000', year: 2023, modelId: 3, plate: '34YVZ555', aiBadge: 'Yüksek Fiyat', aiColor: 'bg-rose-500/20 text-rose-400', features: [3] }
    ];

    const filteredVehicles = initialVehicles.filter(vehicle =>
        selectedFeatures.length === 0 ? true : selectedFeatures.every(fId => vehicle.features.includes(fId))
    );

    const toggleFeature = (id) => {
        setSelectedFeatures(prev => prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]);
    };

    const handlePlateQuery = (e) => {
        e.preventDefault();
        if (plateQuery.toUpperCase() === '34ABC123') {
            setHistoryResult({ hasDamage: false, amount: '0 TL', lastKm: '45.000 KM', status: 'Temiz, Hasarsız Raporu' });
        } else if (plateQuery.toUpperCase() === '06XYZ999') {
            setHistoryResult({ hasDamage: true, amount: '145.000 TL', lastKm: '12.000 KM', status: 'Ağır Hasar Kayıtlı' });
        } else {
            setHistoryResult({ hasDamage: false, amount: '0 TL', lastKm: 'Bilinmiyor', status: 'Sistemde kayıt yok: Temiz (Mock)' });
        }
    };

    const sendMessage = (e) => {
        e.preventDefault();
        if (!newMessage.trim()) return;
        setMessages([...messages, { id: Date.now(), sender: 'Sen', text: newMessage, time: '19:55' }]);
        setNewMessage('');
    };

    return (
        <div className="min-h-screen bg-gray-950 text-gray-100 font-sans flex flex-col">
            <header className="bg-gray-900 border-b border-gray-800 p-4 sticky top-0 z-50 shadow-md">
                <div className="max-w-7xl mx-auto flex justify-between items-center">
                    <h1 className="text-xl font-bold tracking-wider text-indigo-400 flex items-center gap-2">
                        🚗 Arabam<span className="text-white bg-indigo-600 px-1.5 py-0.5 rounded text-sm">TR</span>
                    </h1>
                    <div className="text-xs bg-gray-800 px-3 py-1.5 rounded-full border border-gray-700 text-gray-300">
                        📲 PWA: Masaüstüne Sabitlenebilir Mod Aktif
                    </div>
                </div>
            </header>

            <div className="flex-1 max-w-7xl w-full mx-auto p-4 grid grid-cols-1 lg:grid-cols-4 gap-6">
                <aside className="bg-gray-900 p-4 rounded-2xl border border-gray-800 h-fit shadow-sm">
                    <h3 className="font-bold text-gray-200 mb-4">🛠️ Donanım Filtresi</h3>
                    <div className="space-y-2">
                        {availableFeatures.map(f => (
                            <label key={f.id} className="flex items-center gap-3 p-2 rounded-lg hover:bg-gray-800/60 cursor-pointer transition">
                                <input
                                    type="checkbox"
                                    checked={selectedFeatures.includes(f.id)}
                                    onChange={() => toggleFeature(f.id)}
                                    className="rounded bg-gray-950 border-gray-700 text-indigo-600 focus:ring-indigo-500 w-4 h-4"
                                />
                                <span className="text-sm text-gray-300">{f.name}</span>
                            </label>
                        ))}
                    </div>
                </aside>

                <main className="lg:col-span-2 space-y-6">
                    <h2 className="text-lg font-bold text-gray-300">📋 Güncel Otomobil İlanları ({filteredVehicles.length})</h2>
                    <div className="grid grid-cols-1 gap-4">
                        {filteredVehicles.map(v => (
                            <div
                                key={v.id}
                                onClick={() => { setSelectedVehicle(v); setHistoryResult(null); }}
                                className={`p-4 bg-gray-900 rounded-2xl border transition cursor-pointer flex flex-col justify-between h-44 hover:scale-[1.01] ${selectedVehicle?.id === v.id ? 'border-indigo-500 shadow-lg shadow-indigo-500/10' : 'border-gray-800 hover:border-gray-700'}`}
                            >
                                <div>
                                    <div className="flex justify-between items-start">
                                        <h3 className="font-semibold text-white text-base">{v.title}</h3>
                                        <span className={`text-[11px] font-medium px-2 py-0.5 rounded-full ${v.aiColor}`}>🤖 {v.aiBadge}</span>
                                    </div>
                                    <p className="text-xs text-gray-400 mt-1">Yıl: {v.year} — KM: {v.km}</p>
                                </div>
                                <div className="text-right border-t border-gray-800/60 pt-2 flex justify-between items-center">
                                    <span className="text-xs text-gray-500">Plaka: {v.plate}</span>
                                    <span className="text-lg font-bold text-indigo-400">{v.price}</span>
                                </div>
                            </div>
                        ))}
                    </div>

                    {selectedVehicle && (
                        <div className="bg-gray-900 p-5 rounded-2xl border border-indigo-500/40 shadow-xl">
                            <div className="flex justify-between items-start border-b border-gray-800 pb-3">
                                <div>
                                    <span className="text-xs font-semibold uppercase tracking-wider text-indigo-400">Seçili İlan Detayları</span>
                                    <h3 className="text-xl font-bold text-white mt-1">{selectedVehicle.title}</h3>
                                </div>
                                <button onClick={() => setSelectedVehicle(null)} className="text-gray-500 hover:text-white text-sm">✕ Kapat</button>
                            </div>

                            <div className="mt-4 bg-gray-950 p-4 rounded-xl border border-gray-800">
                                <h4 className="text-xs font-bold text-gray-300 uppercase mb-2">🔍 Ücretsiz Bulut Tramer Sorgusu</h4>
                                <form onSubmit={handlePlateQuery} className="flex gap-2">
                                    <input
                                        type="text"
                                        value={plateQuery}
                                        onChange={(e) => setPlateQuery(e.target.value)}
                                        placeholder="Örn: 34ABC123"
                                        className="flex-1 bg-gray-900 border border-gray-700 rounded-lg px-3 py-1.5 text-sm uppercase text-white focus:outline-none focus:border-indigo-500"
                                    />
                                    <button type="submit" className="bg-indigo-600 hover:bg-indigo-700 px-4 py-1.5 rounded-lg text-xs font-semibold transition">Sorgula</button>
                                </form>

                                {historyResult && (
                                    <div className={`mt-3 p-3 rounded-lg border text-xs ${historyResult.hasDamage ? 'bg-rose-950/40 border-rose-800 text-rose-300' : 'bg-emerald-950/40 border-emerald-800 text-emerald-300'}`}>
                                        <p className="font-bold mb-1">📋 Sorgu Sonucu: {historyResult.status}</p>
                                        <p>Hasar Tutar: {historyResult.amount} | KM: {historyResult.lastKm}</p>
                                    </div>
                                )}
                            </div>

                            <PriceTrendChart modelId={selectedVehicle.modelId} />
                        </div>
                    )}
                </main>

                <aside className="bg-gray-900 p-4 rounded-2xl border border-gray-800 flex flex-col h-[400px] shadow-sm">
                    <div className="border-b border-gray-800 pb-3 mb-3">
                        <h3 className="font-bold text-gray-200">💬 İlan İçi Mesajlaşma</h3>
                        <div className="text-[11px] text-gray-400 mt-0.5 flex items-center gap-1.5">
                            <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span> Çevrimiçi
                        </div>
                    </div>

                    <div className="flex-1 overflow-y-auto space-y-2 pr-1">
                        {messages.map(m => (
                            <div key={m.id} className={`flex flex-col max-w-[85%] ${m.sender === 'Sen' ? 'ml-auto items-end' : 'mr-auto items-start'}`}>
                                <div className={`p-2.5 rounded-2xl text-xs ${m.sender === 'Sen' ? 'bg-indigo-600 text-white rounded-tr-none' : 'bg-gray-800 text-gray-200 rounded-tl-none'}`}>
                                    {m.text}
                                </div>
                            </div>
                        ))}
                    </div>

                    <form onSubmit={sendMessage} className="mt-3 flex gap-2 border-t border-gray-800 pt-3">
                        <input
                            type="text"
                            value={newMessage}
                            onChange={(e) => setNewMessage(e.target.value)}
                            placeholder="Mesajınızı yazın..."
                            className="flex-1 bg-gray-950 border border-gray-800 rounded-xl px-3 py-2 text-xs text-white"
                        />
                        <button type="submit" className="bg-indigo-600 hover:bg-indigo-700 text-white px-3 rounded-xl text-xs transition">Gönder</button>
                    </form>
                </aside>
            </div>
        </div>
    );
}

ReactDOM.createRoot(document.getElementById('root')).render(
    <React.StrictMode>
        <App />
    </React.StrictMode>,
);
