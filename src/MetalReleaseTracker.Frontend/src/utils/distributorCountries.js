const distributorCountries = {
  'osmose productions': '\uD83C\uDDEB\uD83C\uDDF7',
  'drakkar': '\uD83C\uDDE9\uD83C\uDDEA',
  'black metal vendor': '\uD83C\uDDE9\uD83C\uDDEA',
  'black metal store': '\uD83C\uDDE7\uD83C\uDDF7',
  'blackmetalstore': '\uD83C\uDDE7\uD83C\uDDF7',
  'napalm records': '\uD83C\uDDE6\uD83C\uDDF9',
  'napalm': '\uD83C\uDDE6\uD83C\uDDF9',
  'season of mist': '\uD83C\uDDEB\uD83C\uDDF7',
  'paragon records': '\uD83C\uDDFA\uD83C\uDDF8',
  'paragon': '\uD83C\uDDFA\uD83C\uDDF8',
  'werewolf': '\uD83C\uDDF5\uD83C\uDDF1',
  'avantgarde': '\uD83C\uDDEE\uD83C\uDDF9',
};

const distributorCountryNames = {
  'osmose productions': 'France',
  'drakkar': 'Germany',
  'black metal vendor': 'Germany',
  'black metal store': 'Brazil',
  'blackmetalstore': 'Brazil',
  'napalm records': 'Austria',
  'napalm': 'Austria',
  'season of mist': 'France',
  'paragon records': 'USA',
  'paragon': 'USA',
  'werewolf': 'Poland',
  'avantgarde': 'Italy',
};

export const getDistributorCountry = (distributorName) => {
  if (!distributorName) return '';
  const name = distributorName.toLowerCase();
  for (const [key, flag] of Object.entries(distributorCountries)) {
    if (name.includes(key) || key.includes(name)) return flag;
  }
  return '';
};

export const getDistributorCountryName = (distributorName) => {
  if (!distributorName) return '';
  const name = distributorName.toLowerCase();
  for (const [key, countryName] of Object.entries(distributorCountryNames)) {
    if (name.includes(key) || key.includes(name)) return countryName;
  }
  return '';
};
