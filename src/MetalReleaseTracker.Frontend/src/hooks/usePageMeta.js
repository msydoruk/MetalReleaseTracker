import { useEffect } from 'react';

const usePageMeta = (title, description) => {
  useEffect(() => {
    const suffix = 'Metal Release Tracker';
    document.title = title ? `${title} | ${suffix}` : suffix;

    if (description) {
      let meta = document.querySelector('meta[name="description"]');
      if (meta) {
        meta.setAttribute('content', description);
      }
    }

    return () => {
      document.title = suffix;
    };
  }, [title, description]);
};

export default usePageMeta;
