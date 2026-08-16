import React, { useState, useEffect } from 'react';
import { Activity, Signal, Shield, Server } from 'lucide-react';
import { motion } from 'motion/react';
import { useLanguage } from '../contexts/LanguageContext';

type Endpoint = {
  id: string;
  name: string;
  ip: string;
  icon: React.ElementType;
  basePing: number;
  jitter: number;
};

const endpoints: Endpoint[] = [
  { id: 'google', name: 'Google DNS', ip: '8.8.8.8', icon: Activity, basePing: 12, jitter: 4 },
  { id: 'cloudflare', name: 'Cloudflare', ip: '1.1.1.1', icon: Shield, basePing: 9, jitter: 2 },
  { id: 'opendns', name: 'OpenDNS', ip: '208.67.222.222', icon: Server, basePing: 18, jitter: 6 },
  { id: 'aws', name: 'AWS US-East', ip: 'dynamodb.us-east-1.amazonaws.com', icon: Server, basePing: 45, jitter: 12 }
];

export function LatencyMonitorWidget() {
  const { t } = useLanguage();
  
  // Store history of pings for each endpoint (max 20 points)
  const [pingData, setPingData] = useState<Record<string, number[]>>({
    google: Array(20).fill(0),
    cloudflare: Array(20).fill(0),
    opendns: Array(20).fill(0),
    aws: Array(20).fill(0)
  });

  useEffect(() => {
    const interval = setInterval(() => {
      setPingData(prev => {
        const newData = { ...prev };
        endpoints.forEach(ep => {
          const lastPing = prev[ep.id][prev[ep.id].length - 1];
          // Random walk with constraints
          let newPing = lastPing + (Math.random() * ep.jitter - ep.jitter / 2);
          if (newPing < ep.basePing - ep.jitter) newPing = ep.basePing - ep.jitter;
          if (newPing > ep.basePing + ep.jitter * 2) newPing = ep.basePing + ep.jitter * 2;
          
          newData[ep.id] = [...prev[ep.id].slice(1), Math.max(1, newPing)];
        });
        return newData;
      });
    }, 1000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl p-6 mt-6 shadow-sm">
      <div className="flex items-center gap-3 mb-6">
        <div className="p-2.5 bg-blue-100 dark:bg-blue-500/20 text-blue-600 dark:text-blue-500 rounded-xl">
          <Signal className="w-5 h-5" />
        </div>
        <div>
          <h3 className="font-semibold text-lg text-zinc-900 dark:text-zinc-100">{t('latencyMonitorTitle')}</h3>
          <p className="text-sm text-zinc-500 dark:text-zinc-400">{t('latencyMonitorDesc')}</p>
        </div>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full text-left text-sm whitespace-nowrap">
          <thead>
            <tr className="border-b border-zinc-200 dark:border-zinc-800 text-zinc-500 dark:text-zinc-400">
              <th className="pb-3 font-medium">{t('endpoint')}</th>
              <th className="pb-3 font-medium px-4">IP / Host</th>
              <th className="pb-3 font-medium px-4 text-right">Ping</th>
              <th className="pb-3 font-medium px-4 text-right">{t('avgPing')}</th>
              <th className="pb-3 font-medium pl-4">{t('stability')}</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-zinc-100 dark:divide-zinc-800/50">
            {endpoints.map(ep => {
              const history = pingData[ep.id];
              const currentPing = history[history.length - 1];
              const avg = history.reduce((a, b) => a + b, 0) / history.length;
              
              // Calculate stability based on variance
              const variance = history.reduce((acc, val) => acc + Math.pow(val - avg, 2), 0) / history.length;
              const stabilityScore = Math.max(0, 100 - (variance * 2));
              
              let pingColor = 'text-emerald-500';
              if (currentPing > 100) pingColor = 'text-red-500';
              else if (currentPing > 40) pingColor = 'text-amber-500';

              return (
                <tr key={ep.id} className="group">
                  <td className="py-4 font-medium text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                    <ep.icon className="w-4 h-4 text-zinc-400 dark:text-zinc-500 group-hover:text-blue-500 transition-colors" />
                    {ep.name}
                  </td>
                  <td className="py-4 px-4 font-mono text-zinc-500 dark:text-zinc-400 text-xs">
                    {ep.ip}
                  </td>
                  <td className="py-4 px-4 text-right font-mono font-medium">
                    {currentPing > 0 ? (
                      <>
                        <span className={pingColor}>{Math.round(currentPing)}</span>
                        <span className="text-zinc-400 text-xs ml-1">ms</span>
                      </>
                    ) : (
                      <span className="text-zinc-400">--</span>
                    )}
                  </td>
                  <td className="py-4 px-4 text-right font-mono text-zinc-600 dark:text-zinc-300">
                    {avg > 0 ? Math.round(avg) : '--'}
                  </td>
                  <td className="py-4 pl-4 w-32">
                    <div className="flex items-center gap-2">
                      <div className="flex-1 h-2 bg-zinc-100 dark:bg-zinc-800 rounded-full overflow-hidden">
                        <motion.div 
                          className="h-full bg-zinc-300 dark:bg-zinc-700 rounded-full"
                          initial={{ width: 0 }}
                          animate={{ width: avg > 0 ? `${stabilityScore}%` : '0%' }}
                          transition={{ duration: 0.5 }}
                        />
                      </div>
                      <span className="text-xs font-mono text-zinc-500 w-8 text-right">
                        {avg > 0 ? `${Math.round(stabilityScore)}%` : '--'}
                      </span>
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
