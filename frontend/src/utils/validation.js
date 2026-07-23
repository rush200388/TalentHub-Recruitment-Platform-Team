export const limits = {
  name: 50,
  phone: 10,
  location: 120,
  title: 120,
  summary: 2000,
  description: 5000,
  organizationName: 150,
  departmentName: 100,
  website: 300,
  skillsText: 1600,
  skillCount: 30,
  skillLength: 50,
  coverLetter: 3000,
  maxExperience: 60,
  maxSalary: 1_000_000_000,
};

const namePattern =
  /^[\p{L}][\p{L}\p{M}'\- ]{1,49}$/u;

const skillPattern =
  /^[\p{L}\p{N}][\p{L}\p{N} .+#/_\-]{0,49}$/u;

export function validateName(value, label) {
  const trimmed = value.trim();

  if (!trimmed) return `${label} is required`;

  if (!namePattern.test(trimmed)) {
    return `${label} must contain 2–50 letters and may include spaces, apostrophes, or hyphens`;
  }

  return '';
}

export function validatePhone(value) {
  const trimmed = value.trim();

  if (!trimmed) return '';

  if (!/^\d{10}$/.test(trimmed)) {
    return 'Phone number must contain exactly 10 digits';
  }

  return '';
}

export function validateOptionalUrl(value, label) {
  const trimmed = value.trim();
  if (!trimmed) return '';

  try {
    const url = new URL(trimmed);
    if (!['http:', 'https:'].includes(url.protocol)) {
      return `${label} must begin with http:// or https://`;
    }
  } catch {
    return `Enter a valid ${label.toLowerCase()}`;
  }

  return '';
}

export function parseSkills(value) {
  return [...new Set(
    value
      .split(',')
      .map((skill) => skill.trim())
      .filter(Boolean),
  )];
}

export function validateSkills(
  value,
  { required = false } = {},
) {
  const skills = parseSkills(value);

  if (required && skills.length === 0) {
    return 'Add at least one required skill';
  }

  if (skills.length > limits.skillCount) {
    return `Add no more than ${limits.skillCount} skills`;
  }

  const invalid = skills.find(
    (skill) =>
      skill.length > limits.skillLength ||
      !skillPattern.test(skill),
  );

  if (invalid) {
    return `Invalid skill: ${invalid}. Each skill can contain up to 50 letters, numbers, spaces, +, #, ., /, _ or -`;
  }

  return '';
}

export function validateIntegerRange(
  value,
  min,
  max,
  label,
) {
  if (value === '') return `${label} is required`;

  const number = Number(value);

  if (!Number.isInteger(number)) {
    return `${label} must be a whole number`;
  }

  if (number < min || number > max) {
    return `${label} must be between ${min} and ${max}`;
  }

  return '';
}

export function validateMoney(value, label) {
  if (value === '') return '';

  const number = Number(value);

  if (!Number.isFinite(number)) {
    return `${label} must be a valid number`;
  }

  if (number < 0 || number > limits.maxSalary) {
    return `${label} must be between 0 and 1,000,000,000`;
  }

  if (!/^\d+(\.\d{1,2})?$/.test(String(value))) {
    return `${label} can contain at most 2 decimal places`;
  }

  return '';
}

export function todayIso() {
  return new Date().toISOString().slice(0, 10);
}
