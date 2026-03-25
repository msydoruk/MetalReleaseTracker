import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { useSeoConfig } from '../contexts/SeoContext';

const setMetaTag = (attribute, value, content) => {
  let element = document.querySelector(`meta[${attribute}="${value}"]`);
  if (!element) {
    element = document.createElement('meta');
    element.setAttribute(attribute, value);
    document.head.appendChild(element);
  }
  element.setAttribute('content', content);
};

const setHreflangLink = (hreflang, href) => {
  let element = document.querySelector(`link[rel="alternate"][hreflang="${hreflang}"]`);
  if (!element) {
    element = document.createElement('link');
    element.setAttribute('rel', 'alternate');
    element.setAttribute('hreflang', hreflang);
    document.head.appendChild(element);
  }
  element.setAttribute('href', href);
};

const usePageMeta = (title, description, image) => {
  const location = useLocation();
  const seoConfig = useSeoConfig();
  const siteName = seoConfig.SiteName || 'Metal Release Tracker';
  const siteUrl = seoConfig.SiteUrl || 'https://metal-release.com';

  useEffect(() => {
    const effectiveTitle = title || siteName;
    document.title = title ? `${title} | ${siteName}` : siteName;

    if (description) {
      const meta = document.querySelector('meta[name="description"]');
      if (meta) {
        meta.setAttribute('content', description);
      }
    }

    const canonicalUrl = `${siteUrl}${location.pathname}`;

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

    setHreflangLink('en', canonicalUrl);
    setHreflangLink('uk', `${canonicalUrl}${canonicalUrl.includes('?') ? '&' : '?'}lang=uk`);
    setHreflangLink('x-default', canonicalUrl);

    return () => {
      document.title = siteName;
    };
  }, [title, description, image, location.pathname, siteName, siteUrl]);
};

export default usePageMeta;
