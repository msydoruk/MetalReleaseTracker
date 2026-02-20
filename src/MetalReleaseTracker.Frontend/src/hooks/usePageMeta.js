import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';

const usePageMeta = (title, description) => {
  const location = useLocation();

  useEffect(() => {
    const suffix = 'Metal Release Tracker';
    document.title = title ? `${title} | ${suffix}` : suffix;

    if (description) {
      const meta = document.querySelector('meta[name="description"]');
      if (meta) {
        meta.setAttribute('content', description);
      }
    }

    const canonicalUrl = `https://metal-release.com${location.pathname}`;

    let canonical = document.querySelector('link[rel="canonical"]');
    if (canonical) {
      canonical.setAttribute('href', canonicalUrl);
    }

    let ogUrl = document.querySelector('meta[property="og:url"]');
    if (ogUrl) {
      ogUrl.setAttribute('content', canonicalUrl);
    }

    let ogTitle = document.querySelector('meta[property="og:title"]');
    if (ogTitle) {
      ogTitle.setAttribute('content', title || suffix);
    }

    let ogDesc = document.querySelector('meta[property="og:description"]');
    if (ogDesc && description) {
      ogDesc.setAttribute('content', description);
    }

    return () => {
      document.title = suffix;
    };
  }, [title, description, location.pathname]);
};

export default usePageMeta;
