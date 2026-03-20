import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';

const setMetaTag = (attribute, value, content) => {
  let element = document.querySelector(`meta[${attribute}="${value}"]`);
  if (!element) {
    element = document.createElement('meta');
    element.setAttribute(attribute, value);
    document.head.appendChild(element);
  }
  element.setAttribute('content', content);
};

const usePageMeta = (title, description, image) => {
  const location = useLocation();

  useEffect(() => {
    const suffix = 'Metal Release Tracker';
    const effectiveTitle = title || suffix;
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

    setMetaTag('property', 'og:title', effectiveTitle);
    setMetaTag('property', 'og:description', description || '');
    setMetaTag('property', 'og:url', canonicalUrl);
    setMetaTag('property', 'og:type', 'website');

    if (image) {
      setMetaTag('property', 'og:image', image);
    }

    setMetaTag('name', 'twitter:card', image ? 'summary_large_image' : 'summary');
    setMetaTag('name', 'twitter:title', effectiveTitle);
    setMetaTag('name', 'twitter:description', description || '');

    return () => {
      document.title = suffix;
    };
  }, [title, description, image, location.pathname]);
};

export default usePageMeta;
