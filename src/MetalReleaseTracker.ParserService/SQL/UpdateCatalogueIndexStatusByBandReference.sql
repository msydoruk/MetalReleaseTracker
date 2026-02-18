 UPDATE public."CatalogueIndex" ci                                                                                                                   
  SET "Status" = CASE                                                                                                                                 
      WHEN EXISTS (                                                                                                                                   
          SELECT 1
          FROM public."BandReferences" br                                                                                                             
          WHERE LOWER(br."BandName") = LOWER(ci."BandName")
      ) THEN 1
      ELSE 2
  END;